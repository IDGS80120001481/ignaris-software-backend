namespace LignarisBack.Services.email
{
    public class PlantillaSugerencias
    {
        public string templateSuggest()
        {
            return @"
            <!DOCTYPE html>
            <html lang=""es"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Sugerencias - Lignaris</title>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        background-color: #f4f4f9;
                        color: #333;
                        margin: 0;
                        padding: 0;
                    }

                    .container {
                        width: 80%;
                        margin: 0 auto;
                        padding: 20px;
                    }

                    header {
                        background-color: #1e8449;
                        color: white;
                        padding: 20px 0;
                        text-align: center;
                    }

                    h1 {
                        margin: 0;
                    }

                    .suggestions {
                        margin-top: 30px;
                    }

                    .suggestion-card {
                        background-color: white;
                        border-radius: 8px;
                        box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
                        padding: 20px;
                        margin-bottom: 20px;
                    }

                        .suggestion-card h2 {
                            color: #1e8449;
                            margin-top: 0;
                        }

                        .suggestion-card p {
                            font-size: 1.1em;
                            line-height: 1.6;
                        }

                    .ingredient-list {
                        list-style-type: none;
                        padding-left: 0;
                    }

                        .ingredient-list li {
                            background-color: #f9f9f9;
                            padding: 8px;
                            border-radius: 4px;
                            margin-bottom: 5px;
                        }

                    .button {
                        display: inline-block;
                        background-color: #1e8449;
                        color: white;
                        padding: 10px 20px;
                        border-radius: 5px;
                        text-decoration: none;
                        font-weight: bold;
                        margin-top: 15px;
                    }

                    footer {
                        text-align: center;
                        padding: 20px;
                        background-color: #333;
                        color: white;
                    }

                    .pizza-image {
                        width: 100%;
                        height: auto;
                        border-radius: 8px;
                        margin-top: 15px;
                    }
                </style>
            </head>
            <body>
                <header>
                    <h1>Lignaris Pizza</h1>
                    <p>¡Descubre nuestras sugerencias del día y disfruta del delicioso sabor en compañía de tus seres queridos!</p>
                </header>
                <div class=""container"">
                    <section class=""suggestions"">
                        <div class=""suggestion-card"">
                            <h2>Pizza Especial [NOMBRE]</h2>
                            <img src=""[FOTO]"" class=""pizza-image"">
                            <p>Disfruta de nuestra pizza especial con los mejores ingredientes de la temporada. Ideal para compartir con amigos o familiares.</p>
                            <ul class=""ingredient-list"">
                                <li>Tan solo por el precio de: $[PRECIO]</li>
                            </ul>
                            <a href=""#"" class=""button"">¡Pídela ahora!</a>
                        </div>
                    </section>
                </div>
                <footer>
                    <p>&copy; 2024 Pizzería Lignaris. Todos los derechos reservados.</p>
                </footer>
            </body>
            </html>
            ";
        }
    }
}
