namespace Demo

open Aardvark.Vr
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI.Primitives
open Aardvark.SceneGraph.Opc
open OpcViewer.Base.Picking
open OpcViewer.Base.Attributes

[<DomainType>]
type VisibleBox = {
    geometry : Box3d
    color : C4b
    trafo : Trafo3d
    //pos : Pose
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
[<DomainType>]
type ControllerInfo = {
    pose: Pose
    //buttons : ButtonStates
    backButtonPressed : bool
    frontButtonPressed: bool
}

[<DomainType>]
type Model =
    {
        text    : string
        vr      : bool
        
        boxes   : plist<VisibleBox> // maybe change to hmap for finding stuff by id...
        boxHovered : option<string>
        boxSelected : hset<string>

        cameraState : CameraControllerState

        ControllerPosition : V3d
        offsetToCenter : V3d
        isPressed : bool
        
        boxDistance : V3d
        startingLinePos : V3d
        endingLinePos : V3d
        lines : Line3d[]

        //boxes : hmap<string,VisibleBox>
        grabbed : hset<string>
        controllerPositions : hmap<int, ControllerInfo>
        //controllerButtons : hmap<int, bool>
        controllerDistance : float 
        offsetControllerDistance : float
        //opcModel : OpcSelectionViewer.Model
        [<NonIncremental>]
        patchHierarchies     : list<PatchHierarchy> 
        boundingBox          : Box3d
        globalTrafo          : Trafo3d
        initGlobalTrafo      : Trafo3d
        initControlTrafo     : Trafo3d
        opcInfos             : hmap<Box3d, OpcData>
        opcAttributes        : AttributeModel
        mainFrustum          : Frustum
        rotateBox : bool
        pickingModel : PickingModel


    }

module VisibleBox =
    
    let private initial = 
        {
            geometry  = Box3d.FromCenterAndSize(V3d.Zero, V3d.One)
            color = C4b.Red
            trafo = Trafo3d.Identity
            size = V3d.One
            id = ""
        }

    let createVisibleBox (color : C4b) (position : V3d) = 
        {   initial 
                with
                color = color     
                trafo = Trafo3d.Translation(position)
                id = System.Guid.NewGuid().ToString()
        }






