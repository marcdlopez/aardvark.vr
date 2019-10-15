namespace Demo
open System
open System.IO
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.Rendering.Text
open Aardvark.SceneGraph
open Aardvark.SceneGraph.Opc
open Aardvark.UI
open Aardvark.UI.Primitives
open Aardvark.UI.Trafos
open Aardvark.UI.Generic
open FShade
open Aardvark.Application.OpenVR
open Aardvark.Vr

open OpcViewer.Base
open OpcViewer.Base.Picking
open OpcViewer.Base.Attributes

module NavigationOpc = 
    open Aardvark.Application
    open Aardvark.VRVis.Opc
    open Aardvark.UI.Primitives
    open Aardvark.Base.Rendering
    open Model
    open OpenTK

    let currentSceneInfo controllerIndex p model : Model = 
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
                }
                Some  newInfo) // creation 
                
        let newModel = { model with controllerPositions = newControllersPosition}

        let newModel : Model = 
            let controllersFiltered = 
                newModel.controllerPositions
                |> HMap.filter (fun index CI -> 
                    CI.backButtonPressed = true
                )
            match controllersFiltered.Count with
            | 1 ->                    
                let currentControllerTrafo = 
                    newModel
                    |> OpcUtilities.getWorldTrafoIfBackPressed (controllersFiltered |> HMap.keys |> Seq.item 0)

                let newTrafoShift = newModel.initGlobalTrafo * newModel.initControlTrafo.Inverse * currentControllerTrafo
                //printfn "%A" (newTrafoShift.GetModelOrigin())
                {newModel with globalTrafo = newTrafoShift}
            | 2 ->
                let dist = 
                    newModel
                    |> OpcUtilities.getDistanceBetweenControllers (controllersFiltered |> HMap.keys |> Seq.item 0) (controllersFiltered |> HMap.keys |> Seq.item 1)
                    
                let newControllerDistance = dist - newModel.offsetControllerDistance + newModel.initControlTrafo.GetScale()
                //printfn "Distance between Controllers: %f" newControllerDistance
                let scaleControllerCenter = Trafo3d.Translation (-newModel.initControlTrafo.GetModelOrigin()) * Trafo3d.Scale (newControllerDistance) * Trafo3d.Translation (newModel.initControlTrafo.GetModelOrigin())

                // Rotation with origin in the controller
                let firstControllerTrafo = 
                    newModel 
                    |> OpcUtilities.getWorldTrafoIfBackPressed (controllersFiltered |> HMap.keys |> Seq.item 0)

                let secondControllerTrafo = 
                    newModel
                    |> OpcUtilities.getWorldTrafoIfBackPressed (controllersFiltered |> HMap.keys |> Seq.item 1)

                let secondControllerToNewCoordinateSystem = 
                    newModel.rotationAxis * newModel.init2ControlTrafo.Inverse * secondControllerTrafo
                    
                let initialControllerDir = newModel.initControlTrafo.GetModelOrigin() - newModel.init2ControlTrafo.GetModelOrigin()
                let currentControllerDir = firstControllerTrafo.GetModelOrigin() - secondControllerTrafo.GetModelOrigin()
                    
                let getRotation = Trafo3d.RotateInto(initialControllerDir, currentControllerDir)

                let newRotationTrafo = 
                    Trafo3d.Translation (-newModel.rotationAxis.GetModelOrigin()) * getRotation * Trafo3d.Translation (newModel.rotationAxis.GetModelOrigin())
                    // coordinate system (rotation axis) should probably be at the center distance of the controllers
                        
                let newGlobalTrafo = newModel.initGlobalTrafo * newRotationTrafo * scaleControllerCenter//* Trafo3d.Scale (newControllerDistance) 
                printfn "global trafo position : %A" (newGlobalTrafo.GetModelOrigin())
                printfn "rotation coordinate system: %A "(newModel.rotationAxis.GetModelOrigin())
                {newModel with globalTrafo = newGlobalTrafo}
            | _ -> 
                newModel
        newModel

    let initialSceneInfo model : Model = 
        let firstControllerTrafo, secondControllerTrafo, InitialControllerDistance = 
            let controllersFiltered = 
                model.controllerPositions
                |> HMap.filter (fun index CI -> 
                    CI.backButtonPressed = true
                )
            match controllersFiltered.Count with
            | 1 -> 
                model 
                |> OpcUtilities.getWorldTrafoIfBackPressed (controllersFiltered |> HMap.keys |> Seq.item 0), Trafo3d.Identity ,model.offsetControllerDistance 
            | 2 -> 
                let getFirstControllerTrafo = 
                    model 
                    |> OpcUtilities.getWorldTrafoIfBackPressed (controllersFiltered |> HMap.keys |> Seq.item 0)
                let getSecondControllerTrafo = 
                    model 
                    |> OpcUtilities.getWorldTrafoIfBackPressed (controllersFiltered |> HMap.keys |> Seq.item 1)
                let dist = 
                    model 
                    |> OpcUtilities.getDistanceBetweenControllers (controllersFiltered |> HMap.keys |> Seq.item 0) (controllersFiltered |> HMap.keys |> Seq.item 1) 

                getFirstControllerTrafo, getSecondControllerTrafo ,dist
            | _ -> 
                model.initControlTrafo, model.init2ControlTrafo ,model.offsetControllerDistance
       
        let newRotationCoordinateSystem : Trafo3d = 
            let controllerFilter = 
                model.controllerPositions
                |> HMap.filter (fun index CI -> 
                    CI.backButtonPressed = true
                )
            match controllerFilter.Count with
            | 2 -> 
                let getFirstControllerTrafo = 
                    model
                    |> OpcUtilities.getWorldTrafoIfBackPressed (controllerFilter |> HMap.keys |> Seq.item 0)
                let getSecondControllerTrafo = 
                    model
                    |> OpcUtilities.getWorldTrafoIfBackPressed (controllerFilter |> HMap.keys |> Seq.item 1)
                let xAxis : V3d = getSecondControllerTrafo.GetModelOrigin() - getFirstControllerTrafo.GetModelOrigin()
                let newXAxis = xAxis / 2.0
                let yAxisTrafo : Trafo3d = Trafo3d.Translation (newXAxis) * Trafo3d.RotationInDegrees(newXAxis, 90.0)
                let yAxis : V3d = yAxisTrafo.GetModelOrigin()
                let zAxis : V3d = V3d.Cross(newXAxis, yAxis)
                Trafo3d.FromBasis(newXAxis, yAxis, zAxis, getFirstControllerTrafo.GetModelOrigin())
            | _ -> Trafo3d.Identity
        
        {model with initGlobalTrafo = model.globalTrafo; initControlTrafo = firstControllerTrafo; init2ControlTrafo = secondControllerTrafo ;offsetControllerDistance = InitialControllerDistance; rotationAxis = newRotationCoordinateSystem}
