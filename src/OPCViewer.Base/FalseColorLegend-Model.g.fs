namespace OpcViewer.Base.FalseColors

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open OpcViewer.Base.FalseColors

[<AutoOpen>]
module Mutable =

    
    
    type MFalseColorsModel(__initial : OpcViewer.Base.FalseColors.FalseColorsModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<OpcViewer.Base.FalseColors.FalseColorsModel> = Aardvark.Base.Incremental.EqModRef<OpcViewer.Base.FalseColors.FalseColorsModel>(__initial) :> Aardvark.Base.Incremental.IModRef<OpcViewer.Base.FalseColors.FalseColorsModel>
        let _useFalseColors = ResetMod.Create(__initial.useFalseColors)
        let _lowerBound = Aardvark.UI.Mutable.MNumericInput.Create(__initial.lowerBound)
        let _upperBound = Aardvark.UI.Mutable.MNumericInput.Create(__initial.upperBound)
        let _interval = Aardvark.UI.Mutable.MNumericInput.Create(__initial.interval)
        let _invertMapping = ResetMod.Create(__initial.invertMapping)
        let _lowerColor = Aardvark.UI.Mutable.MColorInput.Create(__initial.lowerColor)
        let _upperColor = Aardvark.UI.Mutable.MColorInput.Create(__initial.upperColor)
        let _showColors = ResetMod.Create(__initial.showColors)
        
        member x.useFalseColors = _useFalseColors :> IMod<_>
        member x.lowerBound = _lowerBound
        member x.upperBound = _upperBound
        member x.interval = _interval
        member x.invertMapping = _invertMapping :> IMod<_>
        member x.lowerColor = _lowerColor
        member x.upperColor = _upperColor
        member x.showColors = _showColors :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : OpcViewer.Base.FalseColors.FalseColorsModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_useFalseColors,v.useFalseColors)
                Aardvark.UI.Mutable.MNumericInput.Update(_lowerBound, v.lowerBound)
                Aardvark.UI.Mutable.MNumericInput.Update(_upperBound, v.upperBound)
                Aardvark.UI.Mutable.MNumericInput.Update(_interval, v.interval)
                ResetMod.Update(_invertMapping,v.invertMapping)
                Aardvark.UI.Mutable.MColorInput.Update(_lowerColor, v.lowerColor)
                Aardvark.UI.Mutable.MColorInput.Update(_upperColor, v.upperColor)
                ResetMod.Update(_showColors,v.showColors)
                
        
        static member Create(__initial : OpcViewer.Base.FalseColors.FalseColorsModel) : MFalseColorsModel = MFalseColorsModel(__initial)
        static member Update(m : MFalseColorsModel, v : OpcViewer.Base.FalseColors.FalseColorsModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<OpcViewer.Base.FalseColors.FalseColorsModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module FalseColorsModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let useFalseColors =
                { new Lens<OpcViewer.Base.FalseColors.FalseColorsModel, System.Boolean>() with
                    override x.Get(r) = r.useFalseColors
                    override x.Set(r,v) = { r with useFalseColors = v }
                    override x.Update(r,f) = { r with useFalseColors = f r.useFalseColors }
                }
            let lowerBound =
                { new Lens<OpcViewer.Base.FalseColors.FalseColorsModel, Aardvark.UI.NumericInput>() with
                    override x.Get(r) = r.lowerBound
                    override x.Set(r,v) = { r with lowerBound = v }
                    override x.Update(r,f) = { r with lowerBound = f r.lowerBound }
                }
            let upperBound =
                { new Lens<OpcViewer.Base.FalseColors.FalseColorsModel, Aardvark.UI.NumericInput>() with
                    override x.Get(r) = r.upperBound
                    override x.Set(r,v) = { r with upperBound = v }
                    override x.Update(r,f) = { r with upperBound = f r.upperBound }
                }
            let interval =
                { new Lens<OpcViewer.Base.FalseColors.FalseColorsModel, Aardvark.UI.NumericInput>() with
                    override x.Get(r) = r.interval
                    override x.Set(r,v) = { r with interval = v }
                    override x.Update(r,f) = { r with interval = f r.interval }
                }
            let invertMapping =
                { new Lens<OpcViewer.Base.FalseColors.FalseColorsModel, System.Boolean>() with
                    override x.Get(r) = r.invertMapping
                    override x.Set(r,v) = { r with invertMapping = v }
                    override x.Update(r,f) = { r with invertMapping = f r.invertMapping }
                }
            let lowerColor =
                { new Lens<OpcViewer.Base.FalseColors.FalseColorsModel, Aardvark.UI.ColorInput>() with
                    override x.Get(r) = r.lowerColor
                    override x.Set(r,v) = { r with lowerColor = v }
                    override x.Update(r,f) = { r with lowerColor = f r.lowerColor }
                }
            let upperColor =
                { new Lens<OpcViewer.Base.FalseColors.FalseColorsModel, Aardvark.UI.ColorInput>() with
                    override x.Get(r) = r.upperColor
                    override x.Set(r,v) = { r with upperColor = v }
                    override x.Update(r,f) = { r with upperColor = f r.upperColor }
                }
            let showColors =
                { new Lens<OpcViewer.Base.FalseColors.FalseColorsModel, System.Boolean>() with
                    override x.Get(r) = r.showColors
                    override x.Set(r,v) = { r with showColors = v }
                    override x.Update(r,f) = { r with showColors = f r.showColors }
                }
