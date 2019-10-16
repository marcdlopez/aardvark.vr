namespace Demo

open Aardvark.Base
open Aardvark.Base.Incremental
module MenuOpc = 

    let menuScreen model : Model = 
        let joystickFilter = 
            model.controllerPositions
            |> HMap.filter (fun index CI -> 
                CI.joystickPressed = true
            )
            
        let newModel = 
            match joystickFilter.Count with 
            | 1 -> 
                let controllerPos = joystickFilter |> HMap.values |> Seq.item 0
                let updateBoxPos = 
                    model.boxes
                    |> PList.map (fun x -> 
                        {x with trafo = Trafo3d.Translation(controllerPos.pose.deviceToWorld.GetModelOrigin() + V3d(0.0, 0.0, 0.10))})
                {model with boxes = updateBoxPos}
            | _ -> model
        newModel


