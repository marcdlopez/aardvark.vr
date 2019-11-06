﻿namespace Demo.Main

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
            let newCP = newModel.controllerInfos |> HMap.tryFind controllerPos
            
            match newCP with 
            | Some id -> 
                let updateFlagPos = 
                    model.flagOnController
                    |> PList.map (fun flag -> {flag with trafo = id.pose.deviceToWorld})
                
                match id.backButtonPressed with 
                | true -> 
                    let flagOnController = 
                        newModel.flagOnController
                        |> PList.tryFirst

                    match flagOnController with 
                    | Some flag ->
                        let newFlagOnMars = 
                            newModel.flagOnMars
                            |> PList.prepend flag

                        {newModel with flagOnController = PList.empty; flagOnMars = newFlagOnMars}
                    | None -> newModel
                | false -> {newModel with flagOnController = updateFlagPos}
                
            | None -> newModel
        | DipAndStrike -> 
            newModel
        | Line -> 
            let controllerPos = newModel.menuModel.controllerMenuSelector
            let newCP = newModel.controllerInfos |> HMap.tryFind controllerPos
            
            match newCP with 
            | Some id -> 
                let updateLinePos = 
                    model.lineOnController
                    |> PList.map (fun line -> {line with trafo = id.pose.deviceToWorld})

                match id.backButtonPressed with 
                | true -> 
                    let lineOnController = 
                        newModel.lineOnController
                        |> PList.tryFirst

                    match lineOnController with
                    | Some line -> 
                        let newLineOnMars = 
                            newModel.lineOnMars
                            |> PList.prepend line
                        
                        let newModel = {newModel with lineOnController = PList.empty; lineOnMars = newLineOnMars}

                        let linePointMars = 
                            newModel.lineOnMars
                            |> PList.tryLast
                        match linePointMars with
                        | Some line1 ->
                            let newDistanceLine = V3d.Distance(line1.trafo.GetModelOrigin(), line.trafo.GetModelOrigin())
                            {newModel with lineDistance = newDistanceLine}
                        | None -> newModel
                    | None -> newModel 
                | false -> {newModel with lineOnController = updateLinePos}
            | None -> newModel 
        | Draw -> 
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
            {newModel with globalTrafo = Trafo3d.Translation -model.boundingBox.Center}
        | _ -> newModel
