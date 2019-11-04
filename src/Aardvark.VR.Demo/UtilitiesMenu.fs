namespace Demo
open Aardvark.Vr
open Aardvark.Base

module UtilitiesMenu = 
    open Aardvark.UI
    open Aardvark.Rendering.Text
    open Aardvark.Base.Rendering

    let mkBoxesMenu (controllerPos : Pose) (hmdPos : Pose) (number: int) : plist<VisibleBox> =
            [0..number-1]
            |> List.map (fun x -> 
                VisibleBox.createVisibleBox C4b.Yellow (V3d(controllerPos.deviceToWorld.GetModelOrigin().X, controllerPos.deviceToWorld.GetModelOrigin().Y, controllerPos.deviceToWorld.GetModelOrigin().Z + (0.10 * float x))) hmdPos)
            |> PList.ofList

    