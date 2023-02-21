module App

open Feliz

let inline importDefault x : string = Fable.Core.JsInterop.importDefault x

let thumbnail_srcs = [
    importDefault "./thumbnails/react course.webp", "React Course", "Some Dev", "1.2m views", "2 years ago"
    importDefault "./thumbnails/album.webp", "Some Album", "Some artist", "856k views", "5 years ago"
    importDefault "./thumbnails/juna.webp", "Some Anime OST", "Juna", "564k views", "3 years ago"
    importDefault "./thumbnails/haruhi ost.jpg", "Haruhi OST Track", "Haruhi guys", "224k views", "10 years ago"
    importDefault "./thumbnails/4.webp", "Lawyer Explains Stable Diffusion Lawsuit (Major Implications!)", "Corridor Crew", "1M views", "2 weeks ago"
    importDefault "./thumbnails/5.webp", "This AI changes EVERYTHING (ChatGPT x Blender)", "Stray Creations", "591,323 views", "1 day ago"
    ]


let thumbnail_elem (src, title : string, author : string, stats : string, date : string) =
    Html.div [
        prop.className "video-root-grid"
        prop.children [
            Html.img [prop.className "video"; prop.src src]
            Html.div [
                prop.className "video-desc-grid"
                prop.children [
                    Html.img [prop.className "video-author-icon"; prop.src src]
                    Html.div [
                        prop.className "video-desc-right-grid"
                        prop.children [
                            Html.div [prop.className "video-title"; prop.text title]
                            Html.div [
                                prop.className "video-desc-right-bot-grid video-desc-text"
                                prop.children [
                                    Html.div author
                                    Html.div [
                                        Html.text stats
                                        Html.span [
                                            prop.text "·"
                                            prop.style [style.margin (0,length.em 0.4)]
                                            ]
                                        Html.text date
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

let header =
    Html.div [
        prop.className "header-box"
        prop.children [
            Html.div [
                prop.className "home-box header-item"
                prop.children [
                    Html.img [prop.src (importDefault "./svgs/svgexport-5.svg")]
                    ]
            ]

            Html.div [
                prop.className "outer-search-bar-box"
                prop.children [
                    Html.div [
                        prop.className "search-bar-box"
                        prop.children [
                            Html.input [
                                prop.className "search-bar"
                                prop.type' "text"
                                prop.placeholder "Search"
                                ]
                            Html.img [prop.className "header-item"; prop.src (importDefault "./svgs/svgexport-6.svg"); prop.style [style.padding (0,10)]]
                            ]
                        ]
                    Html.img [prop.className "header-item"; prop.src (importDefault "./svgs/svgexport-9.svg"); prop.style [style.padding (0,5)]]
                    ]
                ]

            Html.div [
                prop.className "profile-box header-item"
                prop.children [
                    Html.img [prop.src (importDefault "./svgs/svgexport-10.svg")]
                    Html.img [prop.src (importDefault "./svgs/svgexport-11.svg")]
                    Html.img [prop.src (importDefault "./svgs/svgexport-12.svg")]
                    ]
                ]
            ]
        ]


let root =
    Html.div [
        header
        Html.div [
            prop.className "videos-grid"
            prop.children (List.map thumbnail_elem thumbnail_srcs)
            ]
        ]

ReactDOM.createRoot(Browser.Dom.document.getElementById "app").render(root)