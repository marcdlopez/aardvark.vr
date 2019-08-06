namespace Demo

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Vr
open Aardvark.UI.Primitives



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

//[<DomainType>]
//type ComplexBox = {
//    position : Trafo3d
//    size : V3d
//    //color : C4b
//    //geometry : Box3d
//    box : VisibleBox
//    //[<TreatAsValue>]
//    //id : string
//}



[<DomainType>]
type Model =
    {
        text    : string
        vr      : bool
        
        boxes   : plist<VisibleBox> // maybe change to hmap for finding stuff by id...
        boxHovered : option<string>
        boxSelected : hset<string>

        cameraState : CameraControllerState

        position : V3d
        isPressed : bool

        //boxes : hmap<string,VisibleBox>
        grabbed : hset<string>
        controllerPositions : hmap<int, Pose>
    }

