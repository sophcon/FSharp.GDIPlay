namespace FSharp.GDIPlay

open System.Windows.Forms

module Utilities =

    let GetFileFromDialog () =
        let dlg = new OpenFileDialog()

        match dlg.ShowDialog() with
        | DialogResult.OK -> Some dlg.FileName
        | _ -> None