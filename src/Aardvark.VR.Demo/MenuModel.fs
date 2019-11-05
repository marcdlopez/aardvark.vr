namespace Demo.Menu

open Aardvark.Base.Incremental
open Aardvark.Vr
open Aardvark.Base
open Demo

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
        boxHovered                  : option<string>
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
    let init = 
        {
            mainMenuBoxes               = PList.empty
            boxHovered                  = None
            subMenuBoxes                = PList.empty
            menu                        = MenuState.Navigation
            subMenu                     = subMenuState.Flag
            initialMenuState            = MenuState.Navigation
            menuButtonPressed           = false
            initialMenuPosition         = Pose.none
            initialMenuPositionBool     = false
            controllerMenuSelector      = ControllerKind.ControllerA
        }