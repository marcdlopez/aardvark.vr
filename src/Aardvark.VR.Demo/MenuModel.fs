namespace Demo

open Aardvark.Base.Incremental
open Aardvark.Vr

type MenuState = 
| Navigation
| Annotation
| MainReset

type subMenuState = 
| Flag
| Reset
| Draw
| Line
| DipAndStrike


[<DomainType>]
type MenuModel = 
    {
        menu                        : MenuState
        subMenu                     : subMenuState
        initialMenuState            : MenuState
        menuButtonPressed           : bool
        initialMenuPosition         : Pose
        initialMenuPositionBool     : bool
        controllerMenuSelector      : ControllerKind

    }

module MenuModel = 
    0