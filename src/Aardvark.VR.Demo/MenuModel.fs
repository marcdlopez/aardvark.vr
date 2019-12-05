namespace Demo.Menu

open Aardvark.Base.Incremental
open Aardvark.Vr
open Aardvark.Base
open Demo

type LineSubMenuState = 
| LineCreate
| EditLine

type HoveredFlagSubmenu = 
| Remove 
| ModifyPos
| InMenu

type FlagSubMenuState = 
| EditFlag 
| FlagCreate

type SubMenuState = 
| Flag of FlagSubMenuState
| Reset
| Draw
| Line of LineSubMenuState
| DipAndStrike
| Init

type MenuState = 
| Navigation
| Annotation of SubMenuState
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
        //subMenu                     : SubMenuState
        lineSubMenu                 : LineSubMenuState
        flagSubMenu                 : FlagSubMenuState
        hoveredFlagMenu             : HoveredFlagSubmenu
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
            //subMenu                     = SubMenuState.Init
            lineSubMenu                 = LineSubMenuState.LineCreate
            flagSubMenu                 = FlagSubMenuState.FlagCreate
            hoveredFlagMenu             = HoveredFlagSubmenu.InMenu
            initialMenuState            = MenuState.Navigation
            menuButtonPressed           = false
            initialMenuPosition         = Pose.none
            initialMenuPositionBool     = false
            controllerMenuSelector      = ControllerInfo.initial
        }