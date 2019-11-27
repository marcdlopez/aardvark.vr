namespace Demo.Main

open Demo
open Demo.Menu
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

module OpcUtilities = 

    let mkFlags (controllerPos : Trafo3d) (number : int) : plist<VisibleBox> = 
        [0..number-1]
        |> List.map (fun x -> 
            VisibleBox.createFlag C4b.White (controllerPos.GetModelOrigin()))
        |> PList.ofList

    let mkSphere (controllerPos : Pose) (number : int) (radius : float) : plist<VisibleSphere> = 
        [0..number-1]
        |> List.map (fun x -> 
            VisibleSphere.createSphere C4b.White (controllerPos.deviceToWorld.GetModelOrigin()) radius)
        |> PList.ofList

    let mkCyllinder (controllerPos : Pose) (number : int) (radius : float) : plist<VisibleCylinder> = 
        [0..number-1]
        |> List.map (fun x -> 
            VisibleCylinder.createCylinder C4b.White (controllerPos.deviceToWorld.GetModelOrigin()) radius)
        |> PList.ofList

    let mkPointDraw (controllerPos : Pose) : VisibleBox =
        VisibleBox.createDrawingPoint C4b.White (controllerPos.deviceToWorld.GetModelOrigin())

    let getWorldTrafoIfBackPressed index model : Trafo3d = 
        let b0 = model.controllerInfos |> HMap.tryFind index
        b0
        |> Option.bind(fun x -> 
            match x.backButtonPressed with
            | true -> Some x.pose.deviceToWorld
            | false -> None)
        |> Option.defaultValue Trafo3d.Identity

    let getDistanceBetweenControllers index0 index1 model : float = 
        let b0 = model.controllerInfos |> HMap.find index0
        let b1 = model.controllerInfos |> HMap.find index1
        let v1 = b0.pose.deviceToWorld.GetModelOrigin()
        let v2 = b1.pose.deviceToWorld.GetModelOrigin()
        V3d.Distance(v1, v2)

    let getTrafoRotation (controlTrafo : Trafo3d) : Trafo3d = 
        let mutable scale = V3d.Zero
        let mutable rot = V3d.Zero
        let mutable trans = V3d.Zero
        controlTrafo.Decompose(&scale, &rot, &trans)
        Trafo3d.Rotation rot

    let updateControllersInfo (kind : ControllerKind) (pose : Pose) (model : Model)= 
        let mkControllerInfo k p = 
            {
                kind               = k
                pose               = p
                buttonKind         = ControllerButtons.Back
                backButtonPressed  = false
                frontButtonPressed = false
                joystickPressed    = false
                joystickHold       = false
                homeButtonPressed  = false
                sideButtonPressed  = false
            }

        model.controllerInfos 
        |> HMap.alter kind (fun old -> 
            match old with 
            | Some x -> 
                Some { x with pose = pose; }   // update / overwrite
            | None -> 
                mkControllerInfo kind pose |> Some // create
        )

    let changeLineSubMenu (model : MenuModel) (lineMode : lineSubMenuState) = 
        {model with lineSubMenu = lineMode}


        