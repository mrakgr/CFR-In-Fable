namespace My
open Feliz

module Html =
    let logo = Fable.Core.JsInterop.importDefault "../assets/react-logo.svg"

    let header =
        Html.header [
            prop.className "header"
            prop.children [
                Html.div [
                    prop.className "header-left"
                    prop.children [
                        Html.img [
                            prop.className "header-logo"
                            prop.src logo
                            ]
                        Html.div [prop.style [style.flexBasis (length.em 1)]]
                        Html.div [
                            prop.className "header-title"
                            prop.text "React Facts"
                            ]
                        ]
                    ]
                Html.div [
                    prop.className "header-about"
                    prop.text "React Course - Project 1"
                    ]
                ]
            ]