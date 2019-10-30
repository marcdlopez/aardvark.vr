namespace Demo

module AnnotationOpc = 
     open Aardvark.Base
     open Aardvark.Base.IndexedGeometryPrimitives

     let annotationMode kind p (annotationSelection : AnnotationMenuState) model : Model = 
    
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
                let lastDrawingBox = //TODO ML do this with a list prepend and tryHead list<'a> -> option<'a>
                    newModel.drawingPoint 
                        |> HMap.values 
                        |> Seq.item (newModel.drawingPoint.Count-1)

                if V3d.Distance(lastDrawingBox.trafo.GetModelOrigin(), c.pose.deviceToWorld.GetModelOrigin()) >= 0.001 then
                    let newDrawingBox = OpcUtilities.mkPointDraw c.pose
                    let updateDrawingPoint = 
                        newModel.drawingPoint
                        |> HMap.add (newModel.drawingPoint.Count + 1) newDrawingBox
                    { newModel with drawingPoint = updateDrawingPoint }
                else newModel
            | _ -> newModel
            
            
        | Reset -> 
            newModel |> OpcUtilities.resetEverything
