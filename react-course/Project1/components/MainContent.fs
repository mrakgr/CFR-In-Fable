namespace My
open Feliz

module Html =

    let main_content =
        Html.div [
            prop.className "main"
            prop.children [
                Html.div [
                    prop.className "main-title"
                    prop.text "Fun Facts About React"
                ]
                Html.div [prop.style [style.flexBasis (length.em 1)]]
                Html.ul [
                    prop.className "main-facts"
                    prop.children [
                        Html.li "Was first released in 2013"
                        Html.li "Was originally created by Jordan Walke"
                        Html.li "Has well over 100k stars on Github"
                        Html.li "Is maintained by Facebook"
                        Html.li "Powers thousands of enterprise apps, including mobile apps"
                        ]
                    ]
                ]
            ]