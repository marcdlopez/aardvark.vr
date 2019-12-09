﻿namespace Demo.Main

open Demo.Menu
open Demo

module AnnotationOpc = 
     open Aardvark.Base
     open Aardvark.Base.IndexedGeometryPrimitives
     open Aardvark.Base

     let annotationMode kind p (annotationSelection : SubMenuState) model : Model = 
    
        let newControllersPosition = 
            model 
            |> OpcUtilities.updateControllersInfo kind p
        
        let newModel = { model with controllerInfos = newControllersPosition}
        
        let ci = newModel.controllerInfos |> HMap.tryFind kind
        
        match annotationSelection with 
        | Flag (flagMenu) -> 
            let controllerPos = newModel.menuModel.controllerMenuSelector
            let newCP = newModel.controllerInfos |> HMap.tryFind controllerPos.kind
            
            match newCP with 
            | Some id -> 
                let newModel = 
                    let updateFlagPos = 
                        newModel.flagOnController
                        |> PList.map (fun flag -> {flag with trafo = id.pose.deviceToWorld})
                
                    match id.backButtonPressed with 
                    | true -> 
                        let flagOnController = 
                            newModel.flagOnController
                            |> PList.tryFirst

                        match flagOnController with 
                        | Some flag ->
                            let updateFlag = {flag with trafo = id.pose.deviceToWorld * newModel.workSpaceTrafo.Inverse}
                            let newFlagOnMars = 
                                newModel.flagOnAnnotationSpace
                                |> PList.prepend updateFlag

                            {newModel with flagOnController = PList.empty; flagOnAnnotationSpace = newFlagOnMars}
                        | None -> newModel
                    | false -> {newModel with flagOnController = updateFlagPos}
               
                let checkFlagHover = 
                    newModel.flagOnAnnotationSpace
                    |> PList.choosei (fun _ f -> 
                        let distance = V3d.Distance(f.trafo.GetModelOrigin(), id.pose.deviceToWorld.GetModelOrigin())
                        if (distance <= 0.1) then 
                            Some {f with color = C4b.Red; flagHovered = true}
                        else 
                            Some {f with color = C4b.White; flagHovered = false}
                    )

                let newModel = {newModel with flagOnAnnotationSpace = checkFlagHover} 
                printfn "annotation mode: %s" (newModel.menuModel.menu.ToString())
                match newModel.menuModel.hoveredFlagMenu with 
                | HoveredFlagSubmenu.Remove -> 
                    let removeFlag = 
                        newModel.flagOnAnnotationSpace
                        |> PList.filter (fun x -> x.flagHovered = false)
                    let newMenuModel = 
                        {
                            newModel.menuModel with 
                                hoveredFlagMenu = HoveredFlagSubmenu.InMenu; 
                                flagSubMenu     = FlagSubMenuState.FlagCreate;
                                menu            = MenuState.Annotation (SubMenuState.Flag FlagSubMenuState.FlagCreate )
                        }
                    {newModel with flagOnAnnotationSpace = removeFlag; menuModel = newMenuModel}
                | HoveredFlagSubmenu.ModifyPos -> //TODO still not finished
                    let hoveredFlag = 
                        newModel.flagOnAnnotationSpace
                        |> PList.filter (fun x -> x.flagHovered = true)
                    let noHoveredFlags = 
                        newModel.flagOnAnnotationSpace
                        |> PList.filter (fun x -> x.flagHovered = false)
                    let newMenuModel = 
                        {
                            newModel.menuModel with 
                                hoveredFlagMenu = HoveredFlagSubmenu.InMenu;
                                flagSubMenu     = FlagSubMenuState.FlagCreate;
                                menu            = MenuState.Annotation (SubMenuState.Flag FlagSubMenuState.FlagCreate )
                        }
                    {
                        newModel with 
                            flagOnController        = hoveredFlag; 
                            flagOnAnnotationSpace   = noHoveredFlags;
                            menuModel               = newMenuModel
                    }
                | _ -> newModel
                
                    

            | None -> newModel
        | DipAndStrike -> 
            let controllerPos = newModel.menuModel.controllerMenuSelector
            let newCP = newModel.controllerInfos |> HMap.tryFind controllerPos.kind
            
            match newCP with 
            | Some id -> 
                let updateCylinderPos = 
                    newModel.dipAndStrikeOnController
                    |> PList.map (fun cylinder -> {cylinder with trafo = id.pose.deviceToWorld})
                    
                let computeDipAndStrikeDotProduct = abs(id.pose.deviceToWorld.Forward.M22) //we only want the M22 element since we are comparing with horizontal plane V3d(OOI)
                // this is the same as line above: let horiPlane = abs(id.pose.deviceToWorld.Forward.C2.Normalized.Z)                
                //printfn "dot product: %f" computeDipAndStrikeDotProduct
                let anglePlane = acos(computeDipAndStrikeDotProduct).DegreesFromRadians()
                //printfn "angle: %f" anglePlane

                let dipColor = 
                    let dipAngle = anglePlane
                    let min = 0.0
                    let max = 90.0

                    let range = new Range1d(min, max)
                    let hue = (dipAngle - range.Min) / range.Size
                    let hsv = HSVf((1.0 - hue) * 0.625, 1.0, 1.0)
                    hsv.ToC3f().ToC4b()

                let newModel = {newModel with dipAndStrikeAngle = anglePlane}

                match id.backButtonPressed with 
                | true -> 
                    let dipAndStrikeOnController = 
                        newModel.dipAndStrikeOnController
                        |> PList.tryFirst

                    match dipAndStrikeOnController with 
                    | Some ds ->
                        let updateDS = 
                            {ds with 
                                trafo = id.pose.deviceToWorld * newModel.workSpaceTrafo.Inverse
                                color = dipColor
                                angle = newModel.dipAndStrikeAngle.ToString()
                                radius = ds.radius
                            }
                        let newDSOnAnnotationSpace = 
                            newModel.dipAndStrikeOnAnnotationSpace
                            |> PList.prepend updateDS

                        {newModel with dipAndStrikeOnController = PList.empty; dipAndStrikeOnAnnotationSpace = newDSOnAnnotationSpace}
                    | None -> newModel
                | false -> {newModel with dipAndStrikeOnController = updateCylinderPos}
                
            | None -> newModel
        | Line(lineMenu) -> 
            let controllerPos = newModel.menuModel.controllerMenuSelector
            let newCP = newModel.controllerInfos |> HMap.tryFind controllerPos.kind
            
            match newCP with 
            | Some id -> 
                let updateLinePos = 
                    newModel.lineOnController
                    |> Option.map (fun sphere -> {sphere with trafo = id.pose.deviceToWorld})

                let newModel = 
                    match id.backButtonPressed with 
                    | true -> 
                        match newModel.lineOnController with
                        | Some newP -> 
                            let newPoint = {newP with trafo = id.pose.deviceToWorld * newModel.workSpaceTrafo.Inverse}
                            printfn"mars line: %A" (newPoint.trafo.GetModelOrigin())
                            
                            let newPointWithDistance = 
                                newModel.lineOnAnnotationSpace
                                |> PList.tryFirst
                                |> Option.map(fun lastP -> 
                                    let dist = System.Math.Round(V3d.Distance(lastP.trafo.GetModelOrigin(), newPoint.trafo.GetModelOrigin()), 3) 
                                    { newPoint with distance = string dist})    // Update distance
                                |> Option.defaultValue newPoint                 // Distance -> "0.0"

                            let newLineOnMars = 
                                newModel.lineOnAnnotationSpace
                                |> PList.prepend newPointWithDistance   // PRE-PREND new POINT!

                            { newModel with
                                lineOnController = None
                                lineOnAnnotationSpace = newLineOnMars
                            }

                        | None -> newModel 
                    | false -> {newModel with lineOnController = updateLinePos}
            
                let checkHover = 
                    newModel.finishedLine
                    |> HMap.map (fun idSphere fl -> 
                        let newSpherePlist = 
                            fl.points
                            |> PList.choosei (fun index s -> 
                                let distance = V3d.Distance(s.pos, id.pose.deviceToWorld.GetModelOrigin())
                                if (distance <= 0.1) then 
                                    Some {s with color = C4b.Red; hovered = true}
                                else
                                    Some {s with color = C4b.White; hovered = false}
                        )
                        {fl with points = newSpherePlist}
                    )
                    
                {newModel with finishedLine = checkHover}
            
            | None -> newModel 
        | Draw -> 
            match ci with
            | Some c when c.backButtonPressed ->
                match newModel.currentlyDrawing with 
                | Some v ->                     
                    let newPointDeviceSpace = c.pose.deviceToWorld.GetModelOrigin()  
                    let newTrafoAnnotationSpace = c.pose.deviceToWorld * newModel.workSpaceTrafo.Inverse
                    let newPointAnnotationSpace = newTrafoAnnotationSpace.GetModelOrigin()

                    let updatedPolygon = 
                        match v.vertices |> PList.tryFirst with 
                        | Some lastInsertedPoint -> 
                            let distance = V3d.Distance(lastInsertedPoint, newPointDeviceSpace) 
                            match distance with
                            | x when x >= 0.001 -> { v with vertices = v.vertices |> PList.prepend newPointAnnotationSpace }  
                            | _ -> v
                        | None -> { v with vertices = PList.single newPointAnnotationSpace }    // updated vertices (only one)

                    { newModel with currentlyDrawing = Some updatedPolygon}
                | None -> 
                    //this case will never happen anyway
                    { newModel with currentlyDrawing = Some {vertices = c.pose.deviceToWorld.GetModelOrigin() |> PList.single } }
                
            | _ -> newModel
        | Reset -> 
            Model.initMainReset
        | _ -> newModel
