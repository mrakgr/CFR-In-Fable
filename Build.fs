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
    run dotnet "fable clean --yes -o output -e .js" clientPath // Delete *.js files created by Fable
)

Target.create "InstallClient" (fun _ -> run npm "install" ".")

Target.create "Bundle" (fun _ ->
    run dotnet $"publish -c Release -o \"{deployPath}\"" serverPath
    run dotnet "fable -o output -s --run npm run build" clientPath
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
    [ "server", dotnet "watch run --mode TestSignalR" serverPath
      "client", dotnet "fable watch -o output -s --run npm run start" clientPath ]
    |> runParallel
)

Target.create "Format" (fun _ ->
    run dotnet "fantomas . -r" "src"
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

]

[<EntryPoint>]
let main args =
    runOrDefault args