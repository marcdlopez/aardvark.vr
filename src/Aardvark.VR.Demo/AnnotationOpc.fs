namespace Demo

module AnnotationOpc = 
     open Aardvark.Base
     open Aardvark.Base.IndexedGeometryPrimitives

     let annotationMode kind p (annotationSelection : subMenuState) model : Model = 
    
        let newControllersPosition = 
            model 
            |> OpcUtilities.updateControllersInfo kind p
        
        let newModel = { model with controllerInfos = newControllersPosition}
        
        match annotationSelection with 
        | Flag -> 
            newModel
        | DipAndStrike -> 
            newModel
        | Line -> 
            newModel
        | Draw -> 
            let ci = newModel.controllerInfos |> HMap.tryFind kind
            match ci with
            | Some c when c.backButtonPressed ->                      
                let lastDrawingBox = 
                    newModel.drawingPoint
                        |> PList.tryFirst
                        
                match lastDrawingBox with 
                | Some box -> 
                    if V3d.Distance(box.trafo.GetModelOrigin(), c.pose.deviceToWorld.GetModelOrigin()) >= 0.001 then
                        let newDrawingBox = OpcUtilities.mkPointDraw c.pose
                        let updateDrawingPoint = 
                            newModel.drawingPoint
                            |> PList.prepend newDrawingBox
                        { newModel with drawingPoint = updateDrawingPoint }
                    else newModel
                | None -> newModel
            | _ -> newModel
            
        | Reset -> 
            newModel |> OpcUtilities.resetEverything
