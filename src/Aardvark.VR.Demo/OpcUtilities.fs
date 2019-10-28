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

module OpcUtilities = 

    //let mkBoxes (number: int) : plist<VisibleBox> =        
    //    [0..number-1]
    //    |> List.map (fun x -> VisibleBox.createVisibleBox C4b.Yellow (V3d(0.0, 2.0 * float x, 0.0)))
    //    |> PList.ofList

    let mkBoxesMenu (controllerPos : Pose) (hmdPos : Pose) (number: int) : plist<VisibleBox> =
        [0..number-1]
        |> List.map (fun x -> 
            VisibleBox.createVisibleBox C4b.Yellow (V3d(controllerPos.deviceToWorld.GetModelOrigin().X, controllerPos.deviceToWorld.GetModelOrigin().Y + 0.25, controllerPos.deviceToWorld.GetModelOrigin().Z + (0.10 * float x))) hmdPos)
        |> PList.ofList

    let mkFlags (controllerPos : Pose) (number : int) : plist<VisibleBox> = 
        [0..number-1]
        |> List.map (fun x -> 
            VisibleBox.createFlag C4b.Magenta (controllerPos.deviceToWorld.GetModelOrigin()))
        |> PList.ofList

    let mkPointDraw (controllerPos : Pose) : VisibleBox =
        VisibleBox.createDrawingPoint C4b.White (controllerPos.deviceToWorld.GetModelOrigin())


    let getWorldTrafoIfBackPressed index model : Trafo3d = 
        let b0 = model.controllerPositions |> HMap.tryFind index
        b0
        |> Option.bind(fun x -> 
            match x.backButtonPressed with
            | true -> Some x.pose.deviceToWorld
            | false -> None)
        |> Option.defaultValue Trafo3d.Identity

    let getDistanceBetweenControllers index0 index1 model : float = 
        let b0 = model.controllerPositions |> HMap.find index0
        let b1 = model.controllerPositions |> HMap.find index1
        let v1 = b0.pose.deviceToWorld.GetModelOrigin()
        let v2 = b1.pose.deviceToWorld.GetModelOrigin()
        V3d.Distance(v1, v2)

    let getTrafoRotation (controlTrafo : Trafo3d) : Trafo3d = 
        let mutable scale = V3d.Zero
        let mutable rot = V3d.Zero
        let mutable trans = V3d.Zero
        controlTrafo.Decompose(&scale, &rot, &trans)
        Trafo3d.Rotation rot

    let mayHover (boxList : plist<VisibleBox>) (controller1 : ControllerInfo) (controller2 : ControllerInfo) = 
        boxList
        |> PList.choose (fun b -> 
            if (b.geometry.Transformed(b.trafo).Contains(controller1.pose.deviceToWorld.GetModelOrigin()) || b.geometry.Transformed(b.trafo).Contains(controller2.pose.deviceToWorld.GetModelOrigin())) then 
                Some b.id
            else None)
        |> PList.tryFirst

    let getControllersInfo index1 index2 model = 
        let controller1 = model.controllerPositions |> HMap.values |> Seq.item index1
        let controller2 = model.controllerPositions |> HMap.values |> Seq.item index2

        controller1, controller2
        