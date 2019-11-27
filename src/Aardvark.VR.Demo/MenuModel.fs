namespace Demo.Menu

open Aardvark.Base.Incremental
open Aardvark.Vr
open Aardvark.Base
open Demo

type lineSubMenuState = 
| LineCreate
| EditLine

type flagSubMenuState = 
| Remove
| EditFlag
| ModifyPos
| FlagCreate

type subMenuState = 
| Flag
| Reset
| Draw
| Line 
| DipAndStrike
| Init

type MenuState = 
| Navigation
| Annotation 
| MainReset

[<DomainType>]
type MenuModel = 
    {
        mainMenuBoxes               : plist<VisibleBox>
        boxHovered                  : option<string>
        subMenuBoxes                : plist<VisibleBox>
        lineSubMenuBoxes            : plist<VisibleBox>
        flagSubMenuBoxes            : plist<VisibleBox>
        menu                        : MenuState
        subMenu                     : subMenuState
        lineSubMenu                 : lineSubMenuState
        flagSubMenu                 : flagSubMenuState
        initialMenuState            : MenuState
        menuButtonPressed           : bool
        initialMenuPosition         : Pose
        initialMenuPositionBool     : bool
        controllerMenuSelector      : ControllerInfo

    }

module MenuModel = 
    let init = 
        {
            mainMenuBoxes               = PList.empty
            boxHovered                  = None
            subMenuBoxes                = PList.empty
            lineSubMenuBoxes            = PList.empty
            flagSubMenuBoxes            = PList.empty
            menu                        = MenuState.Navigation
            subMenu                     = subMenuState.Init
            lineSubMenu                 = lineSubMenuState.LineCreate
            flagSubMenu                 = flagSubMenuState.FlagCreate
            initialMenuState            = MenuState.Navigation
            menuButtonPressed           = false
            initialMenuPosition         = Pose.none
            initialMenuPositionBool     = false
            controllerMenuSelector      = ControllerInfo.initial
        }