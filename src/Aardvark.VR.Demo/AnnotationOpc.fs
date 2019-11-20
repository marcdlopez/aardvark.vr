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
                            newModel.flagOnMars
                            |> PList.prepend updateFlag

                        {newModel with flagOnController = PList.empty; flagOnMars = newFlagOnMars}
                    | None -> newModel
                | false -> {newModel with flagOnController = updateFlagPos}
                
            | None -> newModel
        | DipAndStrike -> 
            newModel
        | Line -> 
            let controllerPos = newModel.menuModel.controllerMenuSelector
            let newCP = newModel.controllerInfos |> HMap.tryFind controllerPos.kind
            
            match newCP with 
            | Some id -> 
                let updateLinePos = 
                    newModel.lineOnController
                    |> PList.map (fun line -> {line with trafo = id.pose.deviceToWorld})

                match id.backButtonPressed with 
                | true -> 
                    let lineOnController = 
                        newModel.lineOnController
                        |> PList.tryFirst

                    match lineOnController with
                    | Some line -> 
                        let updateLine = {line with trafo = id.pose.deviceToWorld * newModel.workSpaceTrafo.Inverse}
                        
                        let newLineOnMars = 
                            newModel.lineOnMars
                            |> PList.prepend updateLine
                        
                        let spherePoint =  
                            newLineOnMars 
                            |> PList.toArray 
                            |> Array.map (fun sphere -> sphere)
                        
                        let sphereLine = 
                            spherePoint 
                            |> Array.pairwise
                            |> Array.map (fun (a, b) -> new Line3d(a.trafo.GetModelOrigin(), b.trafo.GetModelOrigin()))

                        let newModel = {newModel with lineMarsDisplay = sphereLine; lineOnController = PList.empty; lineOnMars = newLineOnMars}

                        let linePointMars = 
                            newLineOnMars
                            |> PList.tryLast

                        match linePointMars with
                        | Some line1 ->
                            let newDistanceLine = V3d.Distance(line1.trafo.GetModelOrigin(), updateLine.trafo.GetModelOrigin())
                            let newSphereListIndex = 
                                newModel.lineOnMars
                                |> PList.findIndex updateLine 
                            let newSphereList = 
                                newModel.lineOnMars
                                |> PList.alter newSphereListIndex (fun dist -> 
                                    match dist with 
                                    | Some s -> Some {s with distance = string newDistanceLine}
                                    | None -> Some VisibleSphere.initial
                                )
                            {newModel with lineOnMars = newSphereList}
                        | None -> newModel
                    | None -> newModel 
                | false -> {newModel with lineOnController = updateLinePos}
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
                            //printfn "on the fly: %A" newPointAnnotationSpace
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
            {newModel with 
                opcSpaceTrafo       = Trafo3d.Translation -model.boundingBox.Center * Trafo3d.RotateInto(model.boundingBox.Center.Normalized, V3d.OOI) 
                annotationSpaceTrafo      = Trafo3d.Identity
                workSpaceTrafo      = Trafo3d.Identity
                flagOnController    = PList.empty
                flagOnMars          = PList.empty
                lineOnController    = PList.empty        
                lineOnMars          = PList.empty
                lineMarsDisplay     = [|Line3d()|]
                drawingLine         = [|Line3d()|]
            }
        | _ -> newModel
