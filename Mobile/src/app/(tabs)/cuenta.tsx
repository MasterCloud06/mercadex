import { router } from 'expo-router';
import { useState } from 'react';
import { Alert, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Aviso, Boton, Campo, Cargando, Tarjeta, Titulo } from '@/componentes/ui';
import { api, type Perfil, type Sesion } from '@/lib/api';
import { useDatos } from '@/lib/useDatos';
import { useSesion } from '@/lib/sesion';
import { colores, espacio } from '@/lib/tema';

export default function Cuenta() {
  const { salir, renovar } = useSesion();
  const { datos, error, cargando } = useDatos(() => api.get<Perfil>('/api/app/perfil'));
  const [cambiando, setCambiando] = useState(false);
  const [actual, setActual] = useState('');
  const [nueva, setNueva] = useState('');
  const [mensaje, setMensaje] = useState<{ texto: string; tono: 'error' | 'ok' } | null>(null);
  const [guardando, setGuardando] = useState(false);

  const cambiar = async () => {
    setMensaje(null);
    if (nueva.length < 8 || !/[A-Za-z]/.test(nueva) || !/\d/.test(nueva))
      return setMensaje({ texto: 'La nueva contraseña debe tener al menos 8 caracteres, con letras y números.', tono: 'error' });

    setGuardando(true);
    try {
      await renovar(await api.post<Sesion>('/api/app/contrasena', { actual, nueva }));
      setActual('');
      setNueva('');
      setCambiando(false);
      setMensaje({ texto: 'Contraseña cambiada.', tono: 'ok' });
    } catch (e) {
      setMensaje({ texto: e instanceof Error ? e.message : 'No se pudo cambiar.', tono: 'error' });
    } finally {
      setGuardando(false);
    }
  };

  const cerrarSesion = () =>
    Alert.alert('Cerrar sesión', 'Tendrás que volver a ingresar con tu carnet y contraseña.', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Cerrar sesión',
        style: 'destructive',
        onPress: async () => {
          await salir();
          router.replace('/bienvenida');
        },
      },
    ]);

  if (cargando && !datos) return <Cargando />;

  return (
    <SafeAreaView edges={['top']} style={{ flex: 1 }}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={{ flex: 1 }}>
        <ScrollView contentContainerStyle={{ padding: espacio.l, paddingBottom: 40 }} keyboardShouldPersistTaps="handled">
          <Titulo>Mi cuenta</Titulo>
          {error ? <Aviso texto={error} /> : null}
          {mensaje ? <Aviso texto={mensaje.texto} tono={mensaje.tono} /> : null}

          {datos ? (
            <Tarjeta>
              <Text style={e.nombre}>
                {datos.nombres} {datos.apellidos}
              </Text>
              <Text style={e.dato}>Carnet {datos.documento}</Text>
              {datos.telefono ? <Text style={e.dato}>Teléfono {datos.telefono}</Text> : null}
            </Tarjeta>
          ) : null}

          <Text style={e.seccion}>Mis casetas</Text>
          <Tarjeta>
            {datos?.unidades.length ? (
              datos.unidades.map((u, i) => (
                <View key={u.id} style={[e.unidad, i > 0 && { borderTopWidth: 1, borderTopColor: '#EEF2F6' }]}>
                  <Text style={e.unidadCodigo}>
                    {u.tipo === 'Comedor' ? 'Comedor' : u.tipo === 'Ambulante' ? 'Ambulante' : 'Caseta'} {u.codigo}
                  </Text>
                  <Text style={e.dato}>{u.bloque ? `Bloque ${u.bloque}` : 'Sin bloque'}</Text>
                </View>
              ))
            ) : (
              <Text style={e.dato}>No figuras como titular de ninguna caseta.</Text>
            )}
          </Tarjeta>

          <Text style={e.seccion}>Seguridad</Text>
          {cambiando ? (
            <Tarjeta>
              <Campo etiqueta="Contraseña actual" value={actual} onChangeText={setActual} secureTextEntry autoCapitalize="none" textContentType="password" />
              <Campo
                etiqueta="Nueva contraseña"
                value={nueva}
                onChangeText={setNueva}
                secureTextEntry
                autoCapitalize="none"
                textContentType="newPassword"
                ayuda="Mínimo 8 caracteres, con letras y números. Se cerrarán tus otras sesiones."
              />
              <View style={{ flexDirection: 'row', gap: espacio.m }}>
                <Boton titulo="Cancelar" variante="secundario" estilo={{ flex: 1 }} onPress={() => setCambiando(false)} />
                <Boton titulo="Guardar" estilo={{ flex: 1 }} cargando={guardando} onPress={cambiar} />
              </View>
            </Tarjeta>
          ) : (
            <Boton titulo="Cambiar contraseña" variante="secundario" icono="key-outline" onPress={() => setCambiando(true)} estilo={{ marginBottom: espacio.m }} />
          )}

          <Boton titulo="Cerrar sesión" variante="peligro" icono="log-out-outline" onPress={cerrarSesion} />
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const e = StyleSheet.create({
  nombre: { fontSize: 19, fontWeight: '700', color: colores.tinta },
  dato: { fontSize: 14, color: colores.textoSuave, marginTop: 3 },
  seccion: { fontSize: 16, fontWeight: '700', color: colores.tinta, marginTop: espacio.l, marginBottom: espacio.s },
  unidad: { paddingVertical: 10 },
  unidadCodigo: { fontSize: 16, fontWeight: '600', color: colores.texto },
});
