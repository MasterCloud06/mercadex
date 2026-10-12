import { useState } from 'react';
import { Pressable, RefreshControl, ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { Aviso, Cargando, Etiqueta, Tarjeta, Titulo, Vacio } from '@/componentes/ui';
import { api, type Recibo } from '@/lib/api';
import { useDatos } from '@/lib/useDatos';
import { colores, dinero, espacio, fecha } from '@/lib/tema';

export default function Recibos() {
  const { datos, error, cargando, recargando, recargar } = useDatos(() => api.get<Recibo[]>('/api/app/recibos'));
  const [abierto, setAbierto] = useState<string | null>(null);

  if (cargando && !datos) return <Cargando />;

  return (
    <SafeAreaView edges={['top']} style={{ flex: 1 }}>
      <ScrollView
        contentContainerStyle={{ padding: espacio.l, paddingBottom: 32 }}
        refreshControl={<RefreshControl refreshing={recargando} onRefresh={recargar} tintColor={colores.azulMedio} />}>
        <Titulo sub="Todos los recibos de tus casetas, pagados en caja o por la app.">Recibos</Titulo>
        {error ? <Aviso texto={error} /> : null}

        {datos?.length === 0 ? (
          <Vacio icono="receipt-outline" titulo="Sin recibos todavía" texto="Cuando pagues una deuda, el recibo aparecerá aquí." />
        ) : null}

        {(datos ?? []).map((r) => {
          const ver = abierto === r.id;
          return (
            <Pressable key={r.id} onPress={() => setAbierto(ver ? null : r.id)} accessibilityRole="button" accessibilityState={{ expanded: ver }}>
              <Tarjeta estilo={r.anulado ? { opacity: 0.7 } : undefined}>
                <View style={e.fila}>
                  <View style={{ flex: 1 }}>
                    <Text style={e.numero}>Recibo N.º {r.numero}</Text>
                    <Text style={e.detalle}>
                      Caseta {r.caseta} · {fecha(r.fecha, true)} · {r.formaPago}
                    </Text>
                  </View>
                  <View style={{ alignItems: 'flex-end' }}>
                    <Text style={[e.total, r.anulado && { textDecorationLine: 'line-through' }]}>{dinero(r.total, r.moneda)}</Text>
                    {r.anulado ? <Etiqueta texto="Anulado" tono="rojo" /> : null}
                  </View>
                  <Ionicons name={ver ? 'chevron-up' : 'chevron-down'} size={18} color={colores.textoTenue} style={{ marginLeft: 8 }} />
                </View>

                {ver ? (
                  <View style={e.detalleCaja}>
                    {r.detalle.map((d, i) => (
                      <View key={i} style={e.linea}>
                        <Text style={e.lineaConcepto}>
                          {d.concepto}
                          {d.periodo ? ` · ${d.periodo}` : ''}
                        </Text>
                        <Text style={e.lineaMonto}>{dinero(d.monto, r.moneda)}</Text>
                      </View>
                    ))}
                  </View>
                ) : null}
              </Tarjeta>
            </Pressable>
          );
        })}
      </ScrollView>
    </SafeAreaView>
  );
}

const e = StyleSheet.create({
  fila: { flexDirection: 'row', alignItems: 'center' },
  numero: { fontSize: 16, fontWeight: '700', color: colores.tinta },
  detalle: { fontSize: 13, color: colores.textoSuave, marginTop: 3 },
  total: { fontSize: 16, fontWeight: '700', color: colores.tinta, marginBottom: 4 },
  detalleCaja: { marginTop: espacio.m, paddingTop: espacio.m, borderTopWidth: 1, borderTopColor: '#EEF2F6', gap: 8 },
  linea: { flexDirection: 'row', justifyContent: 'space-between', gap: 12 },
  lineaConcepto: { flex: 1, fontSize: 14, color: colores.texto },
  lineaMonto: { fontSize: 14, fontWeight: '600', color: colores.texto },
});
