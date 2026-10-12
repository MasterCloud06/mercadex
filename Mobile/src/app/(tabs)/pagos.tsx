import { RefreshControl, ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Aviso, Cargando, Etiqueta, Tarjeta, Titulo, Vacio } from '@/componentes/ui';
import { api, type PagoEnviado } from '@/lib/api';
import { useDatos } from '@/lib/useDatos';
import { colores, dinero, espacio, fecha } from '@/lib/tema';

const estado = {
  pendiente: { texto: 'En revisión', tono: 'ambar' },
  aprobado: { texto: 'Aprobado', tono: 'verde' },
  rechazado: { texto: 'Rechazado', tono: 'rojo' },
} as const;

export default function MisPagos() {
  const { datos, error, cargando, recargando, recargar } = useDatos(() => api.get<PagoEnviado[]>('/api/app/pagos'));

  if (cargando && !datos) return <Cargando />;

  return (
    <SafeAreaView edges={['top']} style={{ flex: 1 }}>
      <ScrollView
        contentContainerStyle={{ padding: espacio.l, paddingBottom: 32 }}
        refreshControl={<RefreshControl refreshing={recargando} onRefresh={recargar} tintColor={colores.azulMedio} />}>
        <Titulo sub="Comprobantes que enviaste desde la app.">Mis pagos</Titulo>
        {error ? <Aviso texto={error} /> : null}

        {datos?.length === 0 ? (
          <Vacio icono="paper-plane-outline" titulo="Aún no enviaste pagos" texto="Cuando pagues desde la pestaña Deudas, aquí verás si la caja lo aprobó." />
        ) : null}

        {(datos ?? []).map((p) => (
          <Tarjeta key={p.id}>
            <View style={e.fila}>
              <View style={{ flex: 1 }}>
                <Text style={e.monto}>{dinero(p.monto, p.moneda)}</Text>
                <Text style={e.detalle}>
                  Caseta {p.caseta} · {p.deudas} {p.deudas === 1 ? 'deuda' : 'deudas'} · {fecha(p.fecha, true)}
                </Text>
              </View>
              <Etiqueta texto={estado[p.estado].texto} tono={estado[p.estado].tono} />
            </View>
            <Text style={e.detalle}>Transacción {p.referencia}</Text>

            {p.estado === 'aprobado' && p.numeroRecibo ? (
              <Text style={[e.nota, { color: colores.verde }]}>Recibo N.º {p.numeroRecibo} emitido. Lo ves en la pestaña Recibos.</Text>
            ) : null}
            {p.estado === 'rechazado' ? (
              <Text style={[e.nota, { color: colores.rojo }]}>Motivo: {p.motivoRechazo}. Tus deudas siguen pendientes; puedes enviar otro comprobante.</Text>
            ) : null}
            {p.estado === 'pendiente' ? <Text style={e.nota}>La caja está verificando el depósito.</Text> : null}
          </Tarjeta>
        ))}
      </ScrollView>
    </SafeAreaView>
  );
}

const e = StyleSheet.create({
  fila: { flexDirection: 'row', alignItems: 'flex-start', gap: 10 },
  monto: { fontSize: 19, fontWeight: '700', color: colores.tinta },
  detalle: { fontSize: 13, color: colores.textoSuave, marginTop: 3 },
  nota: { fontSize: 13.5, lineHeight: 19, color: colores.textoSuave, marginTop: espacio.s },
});
