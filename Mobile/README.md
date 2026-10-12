# Mercadex — app de socios (Android e iOS)

App para que cada socio vea sus deudas y recibos, y pague por QR enviando
la foto del comprobante. La caja aprueba el pago en Mercadex (escritorio,
menú **Pagos por app**) y el recibo se emite solo.

Hecha con Expo (React Native). Un solo código para Android e iOS.

## Probar en tu celular

1. Instala **Expo Go** en el celular (Play Store / App Store).
2. En la PC:

   ```powershell
   cd C:\Mercadex\Proyecto\Mobile
   npm install
   npx expo start
   ```

3. Escanea el QR que aparece con Expo Go (Android) o con la cámara (iPhone).
   El celular y la PC deben estar en la misma red Wi-Fi.

Por defecto la app usa la API de producción. Para usar la API local:

```powershell
# API local escuchando en la red (no solo en 127.0.0.1)
dotnet run --project src\Mercadex.Api --urls http://0.0.0.0:5189
# App apuntando a la IP de la PC (ver con ipconfig)
$env:EXPO_PUBLIC_API_URL="http://192.168.1.20:5189"; npx expo start
```

## Publicar en tiendas

Se compila en la nube con EAS (no hace falta Mac para iOS):

```powershell
npx eas-cli@latest build --platform android
npx eas-cli@latest build --platform ios
```

Requiere cuenta de Expo, Google Play Console (pago único) y Apple Developer (anual).
