open Fake.Core
open Fake.IO
open Farmer
open Farmer.Builders

open Helpers

initializeContext()

let sharedPath = Path.getFullName "src/Shared"
let serverPath = Path.getFullName "src/Server"
let clientPath = Path.getFullName "src/Client"
let deployPath = Path.getFullName "deploy"

Target.create "Clean" (fun q ->
    Shell.cleanDir deployPath
    run dotnet "fable clean --yes" clientPath // Delete *.fs.js files created by Fable
)

Target.create "InstallClient" (fun _ -> run npm "install" ".")

Target.create "Bundle" (fun _ ->
    [ "server", dotnet $"publish -c Release -o \"{deployPath}\"" serverPath
      "client", dotnet "fable -o output -s --run npm run build" clientPath ]
    |> runParallel
)

Target.create "Azure" (fun _ ->
    let web = webApp {
        name "mrakgr-cfr-in-fable"
        operating_system OS.Linux
        runtime_stack Runtime.DotNet70
        zip_deploy "deploy"
    }
    let deployment = arm {
        location Location.WestEurope
        add_resource web
    }

    deployment
    |> Deploy.execute "Web-Apps" Deploy.NoParameters
    |> ignore
)

Target.create "Run" (fun _ ->
    run dotnet "build" sharedPath
    [ "server", dotnet "watch run" serverPath
      "client", dotnet "fable watch -o output -s --run npm run start" clientPath ]
    |> runParallel
)

Target.create "Format" (fun _ ->
    run dotnet "fantomas . -r" "src"
)

Target.create "Project1" (fun _ ->
    let path = Path.getFullName "react-course/Project1"
    run dotnet "fable watch -o output -s --run npm run proj1" path
)

Target.create "Project2" (fun _ ->
    let path = Path.getFullName "react-course/Project2"
    run dotnet "fable watch -o output -s --run npm run proj2" path
)

Target.create "Html1" (fun _ ->
    let path = Path.getFullName "html-css-course/Project1"
    run dotnet "fable watch -o output -s --run npm run html1" path
)

Target.create "Html2" (fun _ ->
    let path = Path.getFullName "html-css-course/Project2"
    run dotnet "fable watch -o output -s --run npm run html2" path
)

open Fake.Core.TargetOperators

let dependencies = [
    "Clean"
        ==> "InstallClient"
        ==> "Bundle"
        ==> "Azure"

    "Clean"
        ==> "InstallClient"
        ==> "Run"

    "Clean"
        ==> "InstallClient"
        ==> "Project1"

    "Clean"
        ==> "InstallClient"
        ==> "Html1"

    "Clean"
        ==> "InstallClient"
        ==> "Html2"

]

[<EntryPoint>]
let main args =
    runOrDefault args