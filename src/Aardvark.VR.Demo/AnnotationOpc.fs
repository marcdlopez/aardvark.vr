namespace Demo.Main

open Demo.Menu
open Demo

module AnnotationOpc = 
     open Aardvark.Base
     open Aardvark.Base.IndexedGeometryPrimitives
     open Aardvark.Base

     let annotationMode kind p (annotationSelection : subMenuState) model : Model = 
    
        let newControllersPosition = 
            model 
            |> OpcUtilities.updateControllersInfo kind p
        
        let newModel = { model with controllerInfos = newControllersPosition}
        
        let ci = newModel.controllerInfos |> HMap.tryFind kind
        
        match annotationSelection with 
        | Flag -> 
            let controllerPos = newModel.menuModel.controllerMenuSelector
            let newCP = newModel.controllerInfos |> HMap.tryFind controllerPos.kind
            
            match newCP with 
            | Some id -> 
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
                
            | None -> newModel
        | DipAndStrike -> 
            let controllerPos = newModel.menuModel.controllerMenuSelector
            let newCP = newModel.controllerInfos |> HMap.tryFind controllerPos.kind
            
            match newCP with 
            | Some id -> 
                let updateCylinderPos = 
                    newModel.dipAndStrikeOnController
                    |> PList.map (fun cylinder -> {cylinder with trafo = id.pose.deviceToWorld})
                
                match id.backButtonPressed with 
                | true -> 
                    let dipAndStrikeOnController = 
                        newModel.dipAndStrikeOnController
                        |> PList.tryFirst

                    match dipAndStrikeOnController with 
                    | Some ds ->
                        let updateDS = {ds with trafo = id.pose.deviceToWorld * newModel.workSpaceTrafo}
                        let newDSOnAnnotationSpace = 
                            newModel.dipAndStrikeOnAnnotationSpace
                            |> PList.prepend updateDS

                        {newModel with dipAndStrikeOnController = PList.empty; dipAndStrikeOnAnnotationSpace = newDSOnAnnotationSpace}
                    | None -> newModel
                | false -> {newModel with dipAndStrikeOnController = updateCylinderPos}
                
            | None -> newModel
        | Line -> 
            let controllerPos = newModel.menuModel.controllerMenuSelector
            let newCP = newModel.controllerInfos |> HMap.tryFind controllerPos.kind
            
            match newCP with 
            | Some id -> 
                let updateLinePos = 
                    newModel.lineOnController
                    |> PList.map (fun line -> {line with trafo = id.pose.deviceToWorld})

                

                let newModel = 
                    match id.backButtonPressed with 
                    | true -> 
                        let lineOnController = 
                            newModel.lineOnController
                            |> PList.tryFirst

                        match lineOnController with
                        | Some line -> 

                            let updateLine = {line with trafo = id.pose.deviceToWorld * newModel.workSpaceTrafo.Inverse}
                        
                            let newLineOnMars = 
                                newModel.lineOnAnnotationSpace
                                |> PList.prepend updateLine
                        
                            let spherePoint =  
                                newLineOnMars 
                                |> PList.toArray 
                                |> Array.map (fun sphere -> sphere)
                        
                            let sphereLine = 
                                spherePoint 
                                |> Array.pairwise
                                |> Array.map (fun (a, b) -> new Line3d(a.trafo.GetModelOrigin(), b.trafo.GetModelOrigin()))

                            let newModel = {newModel with lineMarsDisplay = sphereLine; lineOnController = PList.empty; lineOnAnnotationSpace = newLineOnMars}

                            let linePointMars = 
                                newLineOnMars
                                |> PList.tryLast

                            match linePointMars with
                            | Some line1 ->
                                let newDistanceLine = V3d.Distance(line1.trafo.GetModelOrigin(), updateLine.trafo.GetModelOrigin())
                                let newSphereListIndex = 
                                    newModel.lineOnAnnotationSpace
                                    |> PList.findIndex updateLine 
                                let newSphereList = 
                                    newModel.lineOnAnnotationSpace
                                    |> PList.alter newSphereListIndex (fun dist -> 
                                        match dist with 
                                        | Some s -> Some {s with distance = string newDistanceLine}
                                        | None -> Some VisibleSphere.initial
                                    )
                                {newModel with lineOnAnnotationSpace = newSphereList}
                            | None -> newModel
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
