﻿namespace Demo

module AnnotationOpc = 
    open Aardvark.Base
    open Aardvark.Base.IndexedGeometryPrimitives

    let annotationMode controllerIndex p model : Model = 
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

        //update position of annotationBox to be in the controller position

        newModel

    let createAnnotationBox model : Model =
        let newBox = OpcUtilities.mkAnnotationBoxes 1

        {model with annotationBoxes = newBox}
