import { Ionicons } from '@expo/vector-icons';
import { Redirect } from 'expo-router';
import { Tabs } from 'expo-router/js-tabs';
import { useSesion } from '@/lib/sesion';
import { colores } from '@/lib/tema';

export default function Pestanas() {
  const { sesion, cargando } = useSesion();
  if (!cargando && !sesion) return <Redirect href="/bienvenida" />;

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        sceneStyle: { backgroundColor: colores.superficie },
        tabBarActiveTintColor: colores.azul,
        tabBarInactiveTintColor: colores.textoTenue,
        tabBarStyle: { borderTopColor: colores.borde, backgroundColor: colores.blanco },
        tabBarLabelStyle: { fontSize: 12, fontWeight: '600' },
      }}>
      <Tabs.Screen
        name="index"
        options={{ title: 'Deudas', tabBarIcon: ({ color, size }) => <Ionicons name="wallet-outline" color={color} size={size} /> }}
      />
      <Tabs.Screen
        name="pagos"
        options={{ title: 'Mis pagos', tabBarIcon: ({ color, size }) => <Ionicons name="time-outline" color={color} size={size} /> }}
      />
      <Tabs.Screen
        name="recibos"
        options={{ title: 'Recibos', tabBarIcon: ({ color, size }) => <Ionicons name="receipt-outline" color={color} size={size} /> }}
      />
      <Tabs.Screen
        name="cuenta"
        options={{ title: 'Mi cuenta', tabBarIcon: ({ color, size }) => <Ionicons name="person-circle-outline" color={color} size={size} /> }}
      />
    </Tabs>
  );
}
