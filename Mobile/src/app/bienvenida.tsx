import { router } from 'expo-router';
import { useState } from 'react';
import { KeyboardAvoidingView, Platform, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Aviso, Boton, Campo } from '@/componentes/ui';
import { useSesion } from '@/lib/sesion';
import { colores, espacio, radio } from '@/lib/tema';

type Modo = 'ingresar' | 'activar';

export default function Bienvenida() {
  const { ingresar, activar } = useSesion();
  const [modo, setModo] = useState<Modo>('ingresar');
  const [documento, setDocumento] = useState('');
  const [codigo, setCodigo] = useState('');
  const [contrasena, setContrasena] = useState('');
  const [confirmacion, setConfirmacion] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  const cambiarModo = (m: Modo) => {
    setModo(m);
    setError(null);
    setContrasena('');
    setConfirmacion('');
  };

  const enviar = async () => {
    setError(null);
    if (!documento.trim()) return setError('Escribe tu número de carnet.');

    if (modo === 'activar') {
      if (codigo.replace(/\D/g, '').length !== 8) return setError('El código de activación tiene 8 números.');
      if (contrasena.length < 8 || !/[A-Za-z]/.test(contrasena) || !/\d/.test(contrasena))
        return setError('La contraseña debe tener al menos 8 caracteres, con letras y números.');
      if (contrasena !== confirmacion) return setError('Las contraseñas no coinciden.');
    } else if (!contrasena) {
      return setError('Escribe tu contraseña.');
    }

    setEnviando(true);
    try {
      if (modo === 'activar') await activar(documento.trim(), codigo.replace(/\D/g, ''), contrasena);
      else await ingresar(documento.trim(), contrasena);
      router.replace('/(tabs)');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'No se pudo ingresar.');
    } finally {
      setEnviando(false);
    }
  };

  return (
    <View style={{ flex: 1, backgroundColor: colores.azul }}>
      <SafeAreaView edges={['top']} style={e.cabecera}>
        <View style={e.sello}>
          <Text style={e.selloLetra}>M</Text>
        </View>
        <Text style={e.marca}>Mercadex</Text>
        <Text style={e.lema}>Tu caseta, tus pagos y tus recibos en el celular.</Text>
      </SafeAreaView>

      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={e.hoja}>
        <ScrollView contentContainerStyle={{ padding: espacio.xl, paddingBottom: 48 }} keyboardShouldPersistTaps="handled">
          <View style={e.selector} accessibilityRole="tablist">
            {(['ingresar', 'activar'] as const).map((m) => (
              <Pressable
                key={m}
                accessibilityRole="tab"
                accessibilityState={{ selected: modo === m }}
                onPress={() => cambiarModo(m)}
                style={[e.opcion, modo === m && e.opcionActiva]}>
                <Text style={[e.opcionTexto, modo === m && e.opcionTextoActiva]}>
                  {m === 'ingresar' ? 'Ingresar' : 'Activar mi cuenta'}
                </Text>
              </Pressable>
            ))}
          </View>

          {modo === 'activar' ? (
            <Text style={e.explicacion}>
              Pide tu código de activación en la caja de la asociación. Con tu carnet y ese código creas tu contraseña.
            </Text>
          ) : null}

          {error ? <Aviso texto={error} /> : null}

          <Campo
            etiqueta="Número de carnet"
            value={documento}
            onChangeText={setDocumento}
            autoCapitalize="characters"
            autoCorrect={false}
            placeholder="Ej.: 4567891"
            textContentType="username"
          />

          {modo === 'activar' ? (
            <Campo
              etiqueta="Código de activación"
              value={codigo}
              onChangeText={setCodigo}
              keyboardType="number-pad"
              maxLength={9}
              placeholder="8 números"
              textContentType="oneTimeCode"
            />
          ) : null}

          <Campo
            etiqueta={modo === 'activar' ? 'Crea tu contraseña' : 'Contraseña'}
            value={contrasena}
            onChangeText={setContrasena}
            secureTextEntry
            autoCapitalize="none"
            textContentType={modo === 'activar' ? 'newPassword' : 'password'}
            ayuda={modo === 'activar' ? 'Mínimo 8 caracteres, con letras y números.' : undefined}
          />

          {modo === 'activar' ? (
            <Campo
              etiqueta="Repite la contraseña"
              value={confirmacion}
              onChangeText={setConfirmacion}
              secureTextEntry
              autoCapitalize="none"
              textContentType="newPassword"
            />
          ) : null}

          <Boton
            titulo={modo === 'activar' ? 'Activar y entrar' : 'Ingresar'}
            onPress={enviar}
            cargando={enviando}
            estilo={{ marginTop: espacio.s }}
          />

          {modo === 'ingresar' ? (
            <Text style={e.pie}>¿Olvidaste tu contraseña? Pide un código nuevo en la caja y elige «Activar mi cuenta».</Text>
          ) : null}
        </ScrollView>
      </KeyboardAvoidingView>
    </View>
  );
}

const e = StyleSheet.create({
  cabecera: { paddingHorizontal: espacio.xl, paddingBottom: espacio.xl, paddingTop: espacio.xl },
  sello: { width: 52, height: 52, borderRadius: 14, backgroundColor: colores.maiz, alignItems: 'center', justifyContent: 'center' },
  selloLetra: { fontSize: 28, fontWeight: '800', color: colores.azul },
  marca: { fontSize: 30, fontWeight: '700', color: colores.blanco, marginTop: espacio.l },
  lema: { fontSize: 15, color: '#B9CCDD', marginTop: 4 },
  hoja: { flex: 1, backgroundColor: colores.superficie, borderTopLeftRadius: 24, borderTopRightRadius: 24 },
  selector: {
    flexDirection: 'row',
    backgroundColor: '#E6ECF2',
    borderRadius: radio.m,
    padding: 4,
    marginBottom: espacio.xl,
  },
  opcion: { flex: 1, paddingVertical: 11, alignItems: 'center', borderRadius: 9 },
  opcionActiva: { backgroundColor: colores.blanco },
  opcionTexto: { fontSize: 15, fontWeight: '600', color: colores.textoSuave },
  opcionTextoActiva: { color: colores.tinta },
  explicacion: { fontSize: 14, lineHeight: 20, color: colores.textoSuave, marginBottom: espacio.l },
  pie: { fontSize: 13, lineHeight: 19, color: colores.textoSuave, textAlign: 'center', marginTop: espacio.xl },
});
