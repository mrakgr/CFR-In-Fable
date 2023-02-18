module MyHtml
open Feliz

module prop =
    let dataTarget x = prop.custom("data-target", x)
    let dataToggle x = prop.custom("data-toggle", x)
    let ariaHaspopup (x : bool) = prop.custom("aria-haspopup",x)
    let ariaLabelledby x = prop.custom("aria-labelledby",x)

let navbar =
    Html.nav [
        prop.className "navbar navbar-expand-lg navbar-light bg-light"
        prop.children [
            Html.a [
                prop.className "navbar-brand"
                prop.href "#"
                prop.text "Navbar"
            ]
            Html.button [
                prop.ariaControls "navbarSupportedContent"
                prop.ariaExpanded false
                prop.ariaLabel "Toggle navigation"
                prop.className "navbar-toggler"
                prop.dataTarget "#navbarSupportedContent"
                prop.dataToggle "collapse"
                prop.type' "button"
                prop.children [
                    Html.span [
                        prop.className "navbar-toggler-icon"
                    ]
                ]
            ]
            Html.div [
                prop.className "collapse navbar-collapse"
                prop.id "navbarSupportedContent"
                prop.children [
                    Html.ul [
                        prop.className "navbar-nav mr-auto"
                        prop.children [
                            Html.li [
                                prop.className "nav-item active"
                                prop.children [
                                    Html.a [
                                        prop.className "nav-link"
                                        prop.href "#"
                                        prop.children [
                                            Html.text "Home "
                                            Html.span [
                                                prop.className "sr-only"
                                                prop.text "(current)"
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                            Html.li [
                                prop.className "nav-item"
                                prop.children [
                                    Html.a [
                                        prop.className "nav-link"
                                        prop.href "#"
                                        prop.text "Link"
                                    ]
                                ]
                            ]
                            Html.li [
                                prop.className "nav-item dropdown"
                                prop.children [
                                    Html.a [
                                        prop.ariaExpanded false
                                        prop.ariaHaspopup true
                                        prop.className "nav-link dropdown-toggle"
                                        prop.dataToggle "dropdown"
                                        prop.href "#"
                                        prop.id "navbarDropdown"
                                        prop.role "button"
                                        prop.text "Dropdown "
                                    ]
                                    Html.div [
                                        prop.ariaLabelledby "navbarDropdown"
                                        prop.className "dropdown-menu"
                                        prop.children [
                                            Html.a [
                                                prop.className "dropdown-item"
                                                prop.href "#"
                                                prop.text "Action"
                                            ]
                                            Html.a [
                                                prop.className "dropdown-item"
                                                prop.href "#"
                                                prop.text "Another action"
                                            ]
                                            Html.div [
                                                prop.className "dropdown-divider"
                                            ]
                                            Html.a [
                                                prop.className "dropdown-item"
                                                prop.href "#"
                                                prop.text "Something else here"
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                            Html.li [
                                prop.className "nav-item"
                                prop.children [
                                    Html.a [
                                        prop.className "nav-link disabled"
                                        prop.href "#"
                                        prop.text "Disabled"
                                    ]
                                ]
                            ]
                        ]
                    ]
                    Html.form [
                        prop.className "form-inline my-2 my-lg-0"
                        prop.children [
                            Html.input [
                                prop.className "form-control mr-sm-2"
                                prop.type' "search"
                                prop.placeholder "Search"
                                prop.ariaLabel "Search"
                            ]
                            Html.button [
                                prop.className "btn btn-outline-success my-2 my-sm-0"
                                prop.type' "submit"
                                prop.text "Search"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]