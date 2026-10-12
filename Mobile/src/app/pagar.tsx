import { Ionicons } from '@expo/vector-icons';
import * as Clipboard from 'expo-clipboard';
import { manipulateAsync, SaveFormat } from 'expo-image-manipulator';
import * as ImagePicker from 'expo-image-picker';
import { router, useLocalSearchParams } from 'expo-router';
import { useEffect, useState } from 'react';
import { Alert, Image, KeyboardAvoidingView, Platform, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Aviso, Boton, Campo, Cargando, Tarjeta } from '@/componentes/ui';
import { api, API_URL, encabezadosAuth, type DatosPago } from '@/lib/api';
import { colores, dinero, espacio, radio } from '@/lib/tema';

type Foto = { uri: string; base64: string };

export default function Pagar() {
  const p = useLocalSearchParams<{ ids: string; caseta: string; moneda: string; total: string }>();
  const ids = (p.ids ?? '').split(',').filter(Boolean);
  const total = Number(p.total ?? 0);

  const [datos, setDatos] = useState<DatosPago | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [referencia, setReferencia] = useState('');
  const [foto, setFoto] = useState<Foto | null>(null);
  const [preparandoFoto, setPreparandoFoto] = useState(false);
  const [enviando, setEnviando] = useState(false);
  const [copiado, setCopiado] = useState(false);

  useEffect(() => {
    api.get<DatosPago>('/api/app/datos-pago').then(setDatos).catch((e: Error) => setError(e.message));
  }, []);

  const elegirFoto = async (desde: 'camara' | 'galeria') => {
    setError(null);
    try {
      if (desde === 'camara') {
        const permiso = await ImagePicker.requestCameraPermissionsAsync();
        if (!permiso.granted) {
          setError('Permite el uso de la cámara para fotografiar el comprobante, o elige una captura de la galería.');
          return;
        }
      }

      const opciones: ImagePicker.ImagePickerOptions = { mediaTypes: ['images'], quality: 0.9 };
      const r = desde === 'camara'
        ? await ImagePicker.launchCameraAsync(opciones)
        : await ImagePicker.launchImageLibraryAsync(opciones);

      if (r.canceled || !r.assets?.length) return;

      setPreparandoFoto(true);
      const original = r.assets[0];
      // Reducir a un tamaño legible y liviano para enviarlo rápido.
      const acciones = original.width > 1400 ? [{ resize: { width: 1400 } }] : [];
      const lista = await manipulateAsync(original.uri, acciones, { compress: 0.72, format: SaveFormat.JPEG, base64: true });
      if (!lista.base64) throw new Error('No se pudo preparar la foto.');
      setFoto({ uri: lista.uri, base64: lista.base64 });
    } catch (e) {
      setError(e instanceof Error ? e.message : 'No se pudo usar la foto.');
    } finally {
      setPreparandoFoto(false);
    }
  };

  const enviar = async () => {
    setError(null);
    if (referencia.trim().length < 3) return setError('Escribe el número de transacción que figura en tu comprobante.');
    if (!foto) return setError('Adjunta la foto o captura del comprobante.');

    setEnviando(true);
    try {
      const r = await api.post<{ mensaje: string }>('/api/app/pagos', {
        obligacionIds: ids,
        referencia: referencia.trim(),
        comprobanteBase64: foto.base64,
        comprobanteTipo: 'image/jpeg',
      });
      Alert.alert('Comprobante enviado', r.mensaje, [{ text: 'Ver mis pagos', onPress: () => router.replace('/(tabs)/pagos') }]);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'No se pudo enviar.');
    } finally {
      setEnviando(false);
    }
  };

  const copiarCuenta = async () => {
    if (!datos?.numeroCuenta) return;
    await Clipboard.setStringAsync(datos.numeroCuenta);
    setCopiado(true);
    setTimeout(() => setCopiado(false), 2000);
  };

  if (!datos && !error) return <Cargando texto="Preparando el pago..." />;

  return (
    <SafeAreaView edges={['bottom']} style={{ flex: 1 }}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={{ flex: 1 }}>
        <ScrollView contentContainerStyle={{ padding: espacio.l, paddingBottom: 40 }} keyboardShouldPersistTaps="handled">
          <View style={e.importe}>
            <Text style={e.importeEtiqueta}>
              Caseta {p.caseta} · {ids.length} {ids.length === 1 ? 'deuda' : 'deudas'}
            </Text>
            <Text style={e.importeMonto}>{dinero(total, p.moneda ?? 'BOB')}</Text>
            <Text style={e.importeNota}>Paga exactamente este monto.</Text>
          </View>

          {error ? <Aviso texto={error} /> : null}

          {datos && !datos.disponible ? (
            <Aviso
              tono="info"
              texto="La asociación todavía no cargó sus datos de pago en la app. Puedes pagar en la caja como siempre."
            />
          ) : null}

          {/* Paso 1: pagar */}
          <Text style={e.paso}>1. Paga desde la app de tu banco</Text>
          <Tarjeta>
            {datos?.tieneQr ? (
              <View style={e.qrMarco}>
                <Image
                  source={{ uri: `${API_URL}/api/app/datos-pago/qr`, headers: encabezadosAuth() }}
                  style={e.qr}
                  resizeMode="contain"
                  accessibilityLabel="Código QR de cobro de la asociación"
                />
                <Text style={e.ayuda}>Escanea este QR con la app de tu banco, o haz una captura y súbela desde tu banco.</Text>
              </View>
            ) : null}

            {datos?.numeroCuenta ? (
              <View style={[e.cuenta, datos.tieneQr && { borderTopWidth: 1, borderTopColor: '#EEF2F6', paddingTop: espacio.m }]}>
                <View style={{ flex: 1 }}>
                  <Text style={e.dato}>{datos.banco ?? 'Cuenta bancaria'}</Text>
                  <Text style={e.cuentaNumero} selectable>{datos.numeroCuenta}</Text>
                  {datos.titularCuenta ? <Text style={e.dato}>A nombre de {datos.titularCuenta}</Text> : null}
                </View>
                <Pressable onPress={copiarCuenta} style={e.copiar} accessibilityRole="button" accessibilityLabel="Copiar número de cuenta">
                  <Ionicons name={copiado ? 'checkmark' : 'copy-outline'} size={18} color={colores.azulMedio} />
                  <Text style={e.copiarTexto}>{copiado ? 'Copiado' : 'Copiar'}</Text>
                </Pressable>
              </View>
            ) : null}

            {datos?.instrucciones ? <Text style={[e.ayuda, { marginTop: espacio.m }]}>{datos.instrucciones}</Text> : null}
          </Tarjeta>

          {/* Paso 2: datos del comprobante */}
          <Text style={e.paso}>2. Escribe el número de transacción</Text>
          <Campo
            etiqueta="N.º de transacción del comprobante"
            value={referencia}
            onChangeText={setReferencia}
            autoCapitalize="characters"
            autoCorrect={false}
            maxLength={80}
            placeholder="Figura en el comprobante de tu banco"
          />

          {/* Paso 3: foto */}
          <Text style={e.paso}>3. Adjunta el comprobante</Text>
          {foto ? (
            <Tarjeta estilo={{ padding: espacio.s }}>
              <Image source={{ uri: foto.uri }} style={e.foto} resizeMode="contain" accessibilityLabel="Comprobante adjunto" />
              <Pressable onPress={() => setFoto(null)} style={e.quitar} accessibilityRole="button">
                <Text style={{ color: colores.rojo, fontWeight: '600' }}>Quitar y elegir otra</Text>
              </Pressable>
            </Tarjeta>
          ) : (
            <View style={{ flexDirection: 'row', gap: espacio.m, marginBottom: espacio.l }}>
              <Boton titulo="Tomar foto" variante="secundario" icono="camera-outline" estilo={{ flex: 1 }} cargando={preparandoFoto} onPress={() => elegirFoto('camara')} />
              <Boton titulo="Captura" variante="secundario" icono="image-outline" estilo={{ flex: 1 }} cargando={preparandoFoto} onPress={() => elegirFoto('galeria')} />
            </View>
          )}

          <Boton titulo="Enviar comprobante" onPress={enviar} cargando={enviando} estilo={{ marginTop: espacio.s }} />
          <Text style={[e.ayuda, { textAlign: 'center', marginTop: espacio.m }]}>
            La caja verifica el depósito y emite tu recibo. Mientras tanto, estas deudas quedan «en revisión».
          </Text>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const e = StyleSheet.create({
  importe: { backgroundColor: colores.blanco, borderRadius: radio.l, borderWidth: 1, borderColor: colores.borde, padding: espacio.xl, alignItems: 'center', marginBottom: espacio.l },
  importeEtiqueta: { fontSize: 14, color: colores.textoSuave, fontWeight: '600' },
  importeMonto: { fontSize: 34, fontWeight: '800', color: colores.tinta, marginTop: 4 },
  importeNota: { fontSize: 13, color: colores.ambar, marginTop: 6, fontWeight: '600' },
  paso: { fontSize: 16, fontWeight: '700', color: colores.tinta, marginBottom: espacio.s, marginTop: espacio.s },
  qrMarco: { alignItems: 'center' },
  qr: { width: 240, height: 240, borderRadius: radio.m, backgroundColor: colores.blanco },
  ayuda: { fontSize: 13, lineHeight: 19, color: colores.textoSuave, marginTop: espacio.s, textAlign: 'center' },
  cuenta: { flexDirection: 'row', alignItems: 'center', marginTop: espacio.s },
  dato: { fontSize: 13, color: colores.textoSuave },
  cuentaNumero: { fontSize: 19, fontWeight: '700', color: colores.tinta, marginVertical: 2, letterSpacing: 0.5 },
  copiar: { flexDirection: 'row', alignItems: 'center', gap: 6, paddingHorizontal: 12, paddingVertical: 8, borderRadius: radio.s, borderWidth: 1, borderColor: colores.bordeCampo },
  copiarTexto: { color: colores.azulMedio, fontWeight: '600' },
  foto: { width: '100%', height: 320, borderRadius: radio.s, backgroundColor: '#EEF1F5' },
  quitar: { alignItems: 'center', paddingVertical: 12 },
});
