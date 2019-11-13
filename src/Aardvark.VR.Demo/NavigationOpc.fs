namespace Demo.Main

open Aardvark.Base
open Aardvark.Base.Incremental

module NavigationOpc = 
    open Aardvark.Application
    open Aardvark.VRVis.Opc
    open Aardvark.UI.Primitives
    open Aardvark.Base.Rendering
    open Model
    open OpenTK


    let currentSceneInfo kind p model : Model = 

        let newControllersPosition = 
            model 
            |> OpcUtilities.updateControllersInfo kind p

        let newModel = 
            { model with controllerInfos = newControllersPosition}

        let newModel : Model = 
            let controllersFiltered = 
                newModel.controllerInfos
                |> HMap.filter (fun index CI -> 
                    CI.backButtonPressed = true
                )
            match controllersFiltered.Count with
            | 1 ->                    
                let currentControllerTrafo = 
                    newModel
                    |> OpcUtilities.getWorldTrafoIfBackPressed (controllersFiltered |> HMap.keys |> Seq.item 0)

                let newControllerTrafoShift = newModel.initControllerGlobalTrafo * newModel.initControlTrafo.Inverse * currentControllerTrafo 
                let newTrafoShift = newModel.initGlobalTrafo * newModel.initControlTrafo.Inverse * currentControllerTrafo
                printfn "Controller Trafo : %s" (newControllerTrafoShift.GetModelOrigin().ToString())
                printfn "Global Trafo : %s" (newTrafoShift.GetModelOrigin().ToString())
                {newModel with 
                    globalTrafo             = newTrafoShift; 
                    controllerGlobalTrafo   = newControllerTrafoShift
                }
            | 2 ->
                let dist = 
                    newModel
                    |> OpcUtilities.getDistanceBetweenControllers (controllersFiltered |> HMap.keys |> Seq.item 0) (controllersFiltered |> HMap.keys |> Seq.item 1)
                    
                let newControllerDistance = dist - newModel.offsetControllerDistance + newModel.initControlTrafo.GetScale()

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
                        
                let newControllerGlobalTrafo = newModel.initControllerGlobalTrafo * newRotationTrafo * scaleControllerCenter
                let newGlobalTrafo = newModel.initGlobalTrafo * newRotationTrafo * scaleControllerCenter
                {newModel with  
                    globalTrafo = newGlobalTrafo}
                    //; controllerGlobalTrafo = newControllerGlobalTrafo}
            | _ -> 
                newModel
        newModel

    let initialSceneInfo model : Model = 
        let firstControllerTrafo, secondControllerTrafo, InitialControllerDistance = 
            let controllersFiltered = 
                model.controllerInfos
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
                model.controllerInfos
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
        
        {model with 
            initGlobalTrafo             = model.globalTrafo; 
            initControllerGlobalTrafo   = model.controllerGlobalTrafo; 
            initControlTrafo            = firstControllerTrafo; 
            init2ControlTrafo           = secondControllerTrafo;
            offsetControllerDistance    = InitialControllerDistance; 
            rotationAxis                = newRotationCoordinateSystem
        }
