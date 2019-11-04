namespace Demo

open Aardvark.Base.Incremental
open Aardvark.Vr
open Aardvark.Base

type ControllerKind =
| HMD = 0
| LightHouseA = 1
| LightHouseB = 2
| ControllerA = 3
| ControllerB = 4

module ControllerKind =
    let fromInt i =
        i |> enum<ControllerKind>
    let toInt (vrC : ControllerKind) =
        vrC |> int

type ControllerButtons = 
| Joystick  = 0
| Back      = 1
| Side      = 2

module ControllerButtons = 
    let fromInt i = 
        i |> enum<ControllerButtons>
    let toInt (button : ControllerButtons) = 
        button |> int

[<DomainType>]
type ControllerInfo = {
    kind              : ControllerKind
    buttonKind        : ControllerButtons
    pose              : Pose
    backButtonPressed : bool
    frontButtonPressed: bool
    joystickPressed   : bool
}

[<DomainType>]
type ControllerModel = 
    {
        controllerInfos1     : hmap<ControllerKind, ControllerInfo>
    }

//module ControllerModel = 


