module BitmapFilterData

type BitmapFilterData() =
    member val SourceRedEnabled = false with get, set
    member val SourceGreenEnabled = false with get, set
    member val SourceBlueEnabled = false with get, set
    member val OverlayRedEnabled = false with get, set
    member val OverlayGreenEnabled = false with get, set
    member val OverlayBlueEnabled = false with get, set
    member val SourceRedLevel = 1.0f with get, set
    member val SourceGreenLevel = 1.0f with get, set
    member val SourceBlueLevel = 1.0f with get, set
    member val OverlayRedLevel = 0.0f with get, set
    member val OverlayGreenLevel = 0.0f with get, set
    member val OverlayBlueLevel = 0.0f with get, set
    member val BlendTypeRed = "Add" with get, set
    member val BlendTypeGreen = "Add" with get, set
    member val BlendTypeBlue = "Add" with get, set