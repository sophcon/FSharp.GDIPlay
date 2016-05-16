namespace FSharp.GDIPlay

module App =
    open System

    [<EntryPoint>]
    [<STAThread>]
    let main argv = 
        let form = ColorBreakdownForm.GetForm
        form.ShowDialog() |> ignore
        0