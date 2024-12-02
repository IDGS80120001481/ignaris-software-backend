namespace LignarisBack.Services.email
{
    public class PlantillaCarrito
    {
        public string TemplateCart()
        {
            return @"
            <!DOCTYPE html>
            <html lang=""es"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Carrito de Compras</title>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        background-color: #f8f9fa;
                        color: #333;
                        margin: 0;
                        padding: 0;
                    }
                    .container {
                        width: 80%;
                        margin: 20px auto;
                        background-color: white;
                        padding: 20px;
                        border-radius: 8px;
                        box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                    }
                    h1 {
                        text-align: center;
                        color: #1e8449;
                    }
                    .cart-item {
                        display: flex;
                        align-items: center;
                        justify-content: space-between;
                        border-bottom: 1px solid #ddd;
                        padding: 10px 0;
                    }
                    .cart-item img {
                        width: 100px;
                        height: 100px;
                        object-fit: cover;
                        border-radius: 8px;
                    }
                    .cart-details {
                        flex: 1;
                        margin-left: 20px;
                    }
                    .cart-details h2 {
                        margin: 0;
                        font-size: 1.2em;
                        color: #333;
                    }
                    .cart-details p {
                        margin: 5px 0;
                        font-size: 0.9em;
                        color: #666;
                    }
                    .cart-price {
                        text-align: right;
                    }
                    .cart-price p {
                        margin: 5px 0;
                        font-weight: bold;
                    }
                </style>
            </head>
            <body>
                <div class=""container"">
                    <h1>Carrito de Compras</h1>
                    <p>¡Recuerda que tienes productos en tu carrito de compras!</p>

                    [BODY]
                    
                </div>
            </body>
            </html>

            ";
        }

        public string TemplateProduct()
        {
            return @"
            <div class=""cart-item"">
                <img src=""[FOTO]"" alt=""Producto 1"">
                <div class=""cart-details"">
                    <h2>[NOMBRE]</h2>
                    <p>[CANTIDAD]</p>
                </div>
                <div class=""cart-price"">
                    <p>[PRECIO]</p>
                     <p>[TOTAL]</p>
                </div>
            </div>
            ";
        }
    }
}
