import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { ProveedorSesion } from '@/lib/sesion';
import { colores } from '@/lib/tema';

export default function Raiz() {
  return (
    <SafeAreaProvider>
      <ProveedorSesion>
        <StatusBar style="dark" />
        <Stack
          screenOptions={{
            headerShown: false,
            contentStyle: { backgroundColor: colores.superficie },
          }}>
          <Stack.Screen name="index" />
          <Stack.Screen name="bienvenida" />
          <Stack.Screen name="(tabs)" />
          <Stack.Screen
            name="pagar"
            options={{
              headerShown: true,
              title: 'Pagar',
              headerTintColor: colores.azul,
              headerStyle: { backgroundColor: colores.superficie },
              headerShadowVisible: false,
            }}
          />
        </Stack>
      </ProveedorSesion>
    </SafeAreaProvider>
  );
}
