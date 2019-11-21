namespace Demo
open Aardvark.Vr
open Aardvark.Base

module UtilitiesMenu = 
    open Aardvark.UI

    let mkBoxesMenu (controllerPos : Pose) (hmdPos : Pose) (number: int) : plist<VisibleBox> =
            [0..number-1]
            |> List.map (fun x -> 
                VisibleBox.createVisibleBox C4b.Yellow (V3d(controllerPos.deviceToWorld.GetModelOrigin().X, controllerPos.deviceToWorld.GetModelOrigin().Y, controllerPos.deviceToWorld.GetModelOrigin().Z + (0.10 * float x))) hmdPos)
            |> PList.ofList

    let mayHover (boxList : plist<VisibleBox>) (controller1 : ControllerInfo) (controller2 : ControllerInfo) = 
        boxList
        |> PList.choose (fun b -> 
            if (b.geometry.Transformed(b.trafo).Contains(controller1.pose.deviceToWorld.GetModelOrigin()) || b.geometry.Transformed(b.trafo).Contains(controller2.pose.deviceToWorld.GetModelOrigin())) then 
                Some b.id
            else None)
        |> PList.tryFirst

    let getControllersInfo index1 index2 (controllers : hmap<ControllerKind, ControllerInfo>)= 
        let controller1 = controllers |> HMap.values |> Seq.item index1
        let controller2 = controllers |> HMap.values |> Seq.item index2

        controller1, controller2   
        
     
        