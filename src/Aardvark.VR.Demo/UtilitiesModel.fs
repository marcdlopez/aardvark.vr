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

//[<DomainType>]
//type UtilitiesModel = 
//    {
//        controllerInfos     : hmap<ControllerKind, ControllerInfo>
//    }

//module UtilitiesModel = 
//    let initial = 
//        {
//            controllerInfos = HMap.empty
//        }

[<DomainType>]
type VisibleBox = {
    geometry : Box3d
    color : C4b
    trafo : Trafo3d
    [<TreatAsValue>]
    id : string
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
                geometry = Box3d.FromSize(V3d(0.01, 0.01, 0.15))
        }


[<DomainType>]
type VisibleSphere = {
    geometry : Sphere3d
    color : C4b
    trafo : Trafo3d
    [<TreatAsValue>]
    id : string
}

module VisibleSphere = 
    let initial = 
        {
            geometry = Sphere3d.FromRadius(1.0)
            color = C4b.White
            trafo = Trafo3d.Identity
            id = ""
        }

    let createSphere (color : C4b) (position : V3d) (radius : float) = 
        {
            initial 
                with
                color = color 
                trafo = Trafo3d.Translation(position)
                id = System.Guid.NewGuid().ToString()
                geometry = Sphere3d.FromRadius(radius)
        }
