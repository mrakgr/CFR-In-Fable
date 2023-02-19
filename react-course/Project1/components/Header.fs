namespace My
open Feliz

module Html =
    let logo = Fable.Core.JsInterop.importDefault "../assets/react-logo.svg"

    let header =
        Html.header [
            Html.nav [
                prop.className "nav"
                prop.children [
                    Html.div [
                        Html.img [
                            prop.src logo
                            prop.className "nav-logo"
                            ]
                        Html.strong [
                            prop.className "nav-title"
                            prop.text "ReactFacts"
                            ]
                        ]
                    Html.ol [
                        prop.className "nav-items"
                        prop.children [
                            Html.li "Pricing"
                            Html.li "About"
                            Html.li "Contact"
                            ]
                        ]
                    ]
                ]
            ]