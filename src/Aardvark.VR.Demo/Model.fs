namespace Demo

open Aardvark.Vr
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI.Primitives
open Aardvark.SceneGraph.Opc
open OpcViewer.Base.Picking
open OpcViewer.Base.Attributes
open Aardvark.Rendering.Vulkan

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

[<DomainType>]
type VisibleBox = {
    geometry : Box3d
    color : C4b
    trafo : Trafo3d
    //pos : Pose
    //size : V3d
    [<TreatAsValue>]
    id : string
}

[<DomainType>]
type VisibleCone = {
    geometryCone : Cone3d
    color : C4b
    trafo : Trafo3d
    size : V3d
    [<TreatAsValue>]
    id : string
}

[<DomainType>]
type Line = {
    line : Line3d
    color : C4b
}

type State =
    | Pressed
    | Released
    | Hold

type ButtonStates = {
    front : State
    back : State
}

type MenuState = 
    | Navigation
    | Annotation
    | MainReset

type AnnotationMenuState = 
    | Flag
    | Reset
    | Draw
    | Line
    | DipAndStrike

[<DomainType>]
type ControllerInfo = {
    kind              : ControllerKind
    pose              : Pose
    //buttons : ButtonStates
    backButtonPressed : bool
    frontButtonPressed: bool
    joystickPressed   : bool
}

[<DomainType>]
type Model =
    {
        text    : string
        vr      : bool                

        boxes               : plist<VisibleBox> 
        boxHovered          : option<string>
        boxSelected         : hset<string>
        subMenuBoxes        : plist<VisibleBox> 



        cameraState         : CameraControllerState

        ControllerPosition  : V3d
        offsetToCenter      : V3d
        isPressed           : bool
        
        boxDistance         : V3d
        startingLinePos     : V3d
        endingLinePos       : V3d
        lines               : Line3d[]

        //boxes : hmap<string,VisibleBox>
        grabbed                  : hset<string>
        controllerInfos          : hmap<ControllerKind, ControllerInfo>
        controllerDistance       : float 
        controllerMenuSelector   : ControllerKind
        offsetControllerDistance : float
        //opcModel : OpcSelectionViewer.Model
        [<NonIncremental>]
        patchHierarchies            : list<PatchHierarchy> 
        boundingBox                 : Box3d
        globalTrafo                 : Trafo3d
        initGlobalTrafo             : Trafo3d
        initControlTrafo            : Trafo3d
        init2ControlTrafo           : Trafo3d
        rotationAxis                : Trafo3d
        opcInfos                    : hmap<Box3d, OpcData>
        opcAttributes               : AttributeModel
        mainFrustum                 : Frustum
        rotateBox                   : bool
        pickingModel                : PickingModel
        menu                        : MenuState
        annotationMenu              : AnnotationMenuState
        initialMenuState            : MenuState
        menuButtonPressed           : bool
        initialMenuPosition         : Pose
        initialMenuPositionBool     : bool

        drawingPoint         : hmap<int, VisibleBox> //TODO ML use plist instead
        
    }

module VisibleBox =
    
    let private initial = 
        {
            geometry  = Box3d.FromSize(V3d(0.10, 0.5, 0.05))//Box3d.FromCenterAndSize(V3d.Zero, V3d.One)
            color = C4b.Red
            trafo = Trafo3d.Identity
            //size = V3d.One
            id = ""
        }

    let createVisibleBox (color : C4b) (position : V3d) (rotation : Pose) = 
        let x = (position - rotation.deviceToWorld.GetModelOrigin()).Normalized
        let y = V3d.Cross(x,V3d.OOI)
        let z = V3d.Cross(y,x)
        let newCoordinateSystem = Trafo3d.FromBasis(-x,y,z, position)
        {   initial 
                with
                color = color     
                trafo =  newCoordinateSystem
                id = System.Guid.NewGuid().ToString()
        }

    let createDrawingPoint (color : C4b) (position : V3d)= 
        {
            initial 
                with 
                geometry = Box3d.FromCenterAndSize(V3d.Zero, V3d(0.01,0.01,0.01))
                color = color 
                trafo = Trafo3d.Translation(position) 
                id = System.Guid.NewGuid().ToString()
        }

    let createFlag (color : C4b) (position : V3d) = 
        {
            initial 
                with
                color = color
                trafo = Trafo3d.Translation(position)
                id = System.Guid.NewGuid().ToString()
                geometry = Box3d.FromSize(V3d(0.15, 0.05, 0.05))
        }

//module Initial =
//    let boundingBox = 






