import { Ionicons } from '@expo/vector-icons';
import type { ComponentProps, ReactNode } from 'react';
import {
  ActivityIndicator,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
  type StyleProp,
  type TextInputProps,
  type ViewStyle,
} from 'react-native';
import { colores, espacio, radio } from '@/lib/tema';

type Icono = ComponentProps<typeof Ionicons>['name'];

export function Boton({
  titulo,
  onPress,
  variante = 'primario',
  cargando = false,
  deshabilitado = false,
  icono,
  estilo,
}: {
  titulo: string;
  onPress: () => void;
  variante?: 'primario' | 'secundario' | 'acento' | 'peligro';
  cargando?: boolean;
  deshabilitado?: boolean;
  icono?: Icono;
  estilo?: StyleProp<ViewStyle>;
}) {
  const inactivo = deshabilitado || cargando;
  const fondo =
    variante === 'primario' ? colores.azulMedio
    : variante === 'acento' ? colores.maiz
    : colores.blanco;
  const color =
    variante === 'primario' ? colores.blanco
    : variante === 'acento' ? colores.azul
    : variante === 'peligro' ? colores.rojo
    : colores.azulMedio;

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityState={{ disabled: inactivo, busy: cargando }}
      onPress={onPress}
      disabled={inactivo}
      style={({ pressed }) => [
        estilos.boton,
        { backgroundColor: fondo, opacity: inactivo ? 0.5 : pressed ? 0.85 : 1 },
        variante === 'secundario' || variante === 'peligro' ? estilos.botonBorde : null,
        estilo,
      ]}>
      {cargando ? (
        <ActivityIndicator color={color} />
      ) : (
        <View style={estilos.fila}>
          {icono ? <Ionicons name={icono} size={18} color={color} style={{ marginRight: 8 }} /> : null}
          <Text style={[estilos.botonTexto, { color }]}>{titulo}</Text>
        </View>
      )}
    </Pressable>
  );
}

export function Campo({ etiqueta, ayuda, ...props }: TextInputProps & { etiqueta: string; ayuda?: string }) {
  return (
    <View style={{ marginBottom: espacio.l }}>
      <Text style={estilos.etiqueta}>{etiqueta}</Text>
      <TextInput
        placeholderTextColor={colores.textoTenue}
        {...props}
        style={[estilos.campo, props.multiline ? { minHeight: 90, textAlignVertical: 'top', paddingTop: 12 } : null, props.style]}
      />
      {ayuda ? <Text style={estilos.ayuda}>{ayuda}</Text> : null}
    </View>
  );
}

export function Tarjeta({ children, estilo }: { children: ReactNode; estilo?: StyleProp<ViewStyle> }) {
  return <View style={[estilos.tarjeta, estilo]}>{children}</View>;
}

export function Etiqueta({ texto, tono }: { texto: string; tono: 'verde' | 'ambar' | 'rojo' | 'gris' }) {
  const t = {
    verde: [colores.verdeFondo, colores.verde],
    ambar: [colores.ambarFondo, colores.ambar],
    rojo: [colores.rojoFondo, colores.rojo],
    gris: ['#EEF1F5', colores.textoSuave],
  }[tono];
  return (
    <View style={[estilos.etiquetaChip, { backgroundColor: t[0] }]}>
      <Text style={{ color: t[1], fontSize: 12.5, fontWeight: '600' }}>{texto}</Text>
    </View>
  );
}

export function Aviso({ texto, tono = 'error' }: { texto: string; tono?: 'error' | 'info' | 'ok' }) {
  const t = tono === 'error' ? [colores.rojoFondo, colores.rojo] : tono === 'ok' ? [colores.verdeFondo, colores.verde] : ['#E8F0F8', colores.azulMedio];
  return (
    <View style={[estilos.aviso, { backgroundColor: t[0] }]}>
      <Text style={{ color: t[1], fontSize: 14, lineHeight: 20 }}>{texto}</Text>
    </View>
  );
}

export function Cargando({ texto = 'Cargando...' }: { texto?: string }) {
  return (
    <View style={estilos.centro}>
      <ActivityIndicator size="large" color={colores.azulMedio} />
      <Text style={[estilos.ayuda, { marginTop: 12 }]}>{texto}</Text>
    </View>
  );
}

export function Vacio({ icono, titulo, texto }: { icono: Icono; titulo: string; texto: string }) {
  return (
    <View style={[estilos.centro, { paddingVertical: 48 }]}>
      <Ionicons name={icono} size={44} color={colores.textoTenue} />
      <Text style={estilos.vacioTitulo}>{titulo}</Text>
      <Text style={[estilos.ayuda, { textAlign: 'center', maxWidth: 300 }]}>{texto}</Text>
    </View>
  );
}

export function Titulo({ children, sub }: { children: ReactNode; sub?: string }) {
  return (
    <View style={{ marginBottom: espacio.l }}>
      <Text style={estilos.titulo}>{children}</Text>
      {sub ? <Text style={[estilos.ayuda, { marginTop: 4, fontSize: 14 }]}>{sub}</Text> : null}
    </View>
  );
}

export const estilos = StyleSheet.create({
  fila: { flexDirection: 'row', alignItems: 'center' },
  boton: { minHeight: 50, borderRadius: radio.m, paddingHorizontal: espacio.l, alignItems: 'center', justifyContent: 'center' },
  botonBorde: { borderWidth: 1, borderColor: colores.bordeCampo },
  botonTexto: { fontSize: 16, fontWeight: '600' },
  etiqueta: { fontSize: 13, fontWeight: '600', color: '#4A5E73', marginBottom: 6 },
  campo: {
    minHeight: 50,
    borderWidth: 1,
    borderColor: colores.bordeCampo,
    borderRadius: radio.m,
    paddingHorizontal: 14,
    fontSize: 16,
    color: colores.texto,
    backgroundColor: colores.blanco,
  },
  ayuda: { fontSize: 12.5, color: colores.textoSuave, marginTop: 6 },
  tarjeta: {
    backgroundColor: colores.blanco,
    borderRadius: radio.l,
    borderWidth: 1,
    borderColor: colores.borde,
    padding: espacio.l,
    marginBottom: espacio.m,
  },
  etiquetaChip: { alignSelf: 'flex-start', borderRadius: 999, paddingHorizontal: 10, paddingVertical: 3 },
  aviso: { borderRadius: radio.m, padding: espacio.m, marginBottom: espacio.l },
  centro: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: espacio.xl },
  vacioTitulo: { fontSize: 17, fontWeight: '600', color: colores.tinta, marginTop: 12 },
  titulo: { fontSize: 26, fontWeight: '700', color: colores.tinta },
});
