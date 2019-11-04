namespace Demo

open Aardvark.Base.Incremental
open Aardvark.Vr
open Aardvark.Base

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
        mainMenuBoxes               : plist<VisibleBox>
        subMenuBoxes                : plist<VisibleBox>
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