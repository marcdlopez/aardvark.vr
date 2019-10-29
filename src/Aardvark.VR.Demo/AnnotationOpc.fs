namespace Demo

module AnnotationOpc = 
    open Aardvark.Base
    open Aardvark.Base.IndexedGeometryPrimitives

    let annotationMode controllerIndex p (annotationSelection : AnnotationMenuState) model : Model = 
    
        let newControllersPosition = 
            model.controllerPositions |> HMap.alter controllerIndex (fun old -> 
            match old with 
            | Some x -> 
                Some {x with pose = p; }   // update / overwrite
            | None -> 
                let newInfo = {
                    pose = p
                    //buttons  = ButtonStates.
                    backButtonPressed = false
                    frontButtonPressed = false
                    joystickPressed = false
                }
                Some  newInfo) // creation 
                
        let newModel = { model with controllerPositions = newControllersPosition}
        
        match annotationSelection with 
        | Flag -> 
            newModel
        | DipAndStrike -> 
            newModel
        | Line -> 
            newModel
        | Draw -> 
            let ci = newModel.controllerPositions |> HMap.values |> Seq.item controllerIndex
            match ci.backButtonPressed with
            | true -> 
                let lastDrawingBox = newModel.drawingPoint |> HMap.values |> Seq.item (newModel.drawingPoint.Count-1)
                if V3d.Distance(lastDrawingBox.trafo.GetModelOrigin(), ci.pose.deviceToWorld.GetModelOrigin()) >= 0.001 then
                    let newDrawingBox = OpcUtilities.mkPointDraw ci.pose
                    let updateDrawingPoint = 
                        newModel.drawingPoint
                        |> HMap.add (newModel.drawingPoint.Count + 1) newDrawingBox
                    {newModel with drawingPoint = updateDrawingPoint}
                else newModel
            | false -> newModel
            
            
        | Reset -> 
            newModel |> OpcUtilities.resetEverything
