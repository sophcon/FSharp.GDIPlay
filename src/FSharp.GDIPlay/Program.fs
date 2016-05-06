namespace FSharp.GDIPlay

module App =
    open System

    [<EntryPoint>]
    [<STAThread>]
    let main argv = 
        printfn "%A" argv

        match Utilities.GetFileFromDialog() with
        | Some path -> 
            let redImage = Tools.testRed path
            redImage.Save(@"c:\tmp\test.jpg", System.Drawing.Imaging.ImageFormat.Jpeg)
            0
        | None -> 0
