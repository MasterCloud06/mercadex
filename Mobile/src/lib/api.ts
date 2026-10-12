import Constants from 'expo-constants';

/**
 * Dirección de la API. En desarrollo se puede cambiar con
 * EXPO_PUBLIC_API_URL (p. ej. http://192.168.1.20:5189).
 */
export const API_URL: string = (
  process.env.EXPO_PUBLIC_API_URL ??
  (Constants.expoConfig?.extra as { apiUrl?: string } | undefined)?.apiUrl ??
  'https://mercadex-api.onrender.com'
).replace(/\/$/, '');

export class ErrorApi extends Error {
  constructor(mensaje: string, public readonly estado: number) {
    super(mensaje);
  }
}

let tokenActual: string | null = null;
let alExpirar: (() => void) | null = null;

export function configurarSesion(token: string | null, onExpira?: () => void) {
  tokenActual = token;
  if (onExpira) alExpirar = onExpira;
}

export function encabezadosAuth(): Record<string, string> {
  return tokenActual ? { Authorization: `Bearer ${tokenActual}` } : {};
}

async function solicitar<T>(metodo: string, ruta: string, cuerpo?: unknown): Promise<T> {
  let respuesta: Response;

  try {
    respuesta = await fetch(`${API_URL}${ruta}`, {
      method: metodo,
      headers: {
        Accept: 'application/json',
        ...(cuerpo !== undefined ? { 'Content-Type': 'application/json' } : {}),
        ...encabezadosAuth(),
      },
      body: cuerpo !== undefined ? JSON.stringify(cuerpo) : undefined,
    });
  } catch {
    throw new ErrorApi('Sin conexión con Mercadex. Revisa tu Internet e intenta de nuevo.', 0);
  }

  const texto = await respuesta.text();
  let datos: unknown = null;
  try {
    datos = texto ? JSON.parse(texto) : null;
  } catch {
    datos = null;
  }

  if (!respuesta.ok) {
    const mensaje =
      (datos as { mensaje?: string } | null)?.mensaje ??
      (respuesta.status >= 500
        ? 'El servidor tuvo un problema. Intenta en unos minutos.'
        : `No se pudo completar (error ${respuesta.status}).`);

    // Sesión vencida o cerrada desde la caja: volver al inicio de sesión.
    if (respuesta.status === 401 && tokenActual && ruta !== '/api/app/login') {
      alExpirar?.();
    }

    throw new ErrorApi(mensaje, respuesta.status);
  }

  return datos as T;
}

export const api = {
  get: <T>(ruta: string) => solicitar<T>('GET', ruta),
  post: <T>(ruta: string, cuerpo: unknown = {}) => solicitar<T>('POST', ruta, cuerpo),
};

// ------------------------------------------------------------ Tipos

export type Sesion = { token: string; expira: string; nombres: string; apellidos: string };

export type Perfil = {
  nombres: string;
  apellidos: string;
  documento: string;
  telefono: string | null;
  unidades: { id: string; codigo: string; bloque: string | null; tipo: string }[];
};

export type Deuda = {
  id: string;
  concepto: string;
  periodo: string | null;
  monto: number;
  monedaId: string;
  moneda: string;
  fechaEmision: string;
  fechaVencimiento: string | null;
  enRevision: boolean;
};

export type DeudasUnidad = { unidadId: string; codigo: string; bloque: string | null; deudas: Deuda[] };

export type Recibo = {
  id: string;
  numero: number;
  caseta: string;
  fecha: string;
  anulado: boolean;
  moneda: string;
  total: number;
  formaPago: string;
  detalle: { concepto: string; periodo: string | null; monto: number }[];
};

export type DatosPago = {
  disponible: boolean;
  tieneQr: boolean;
  banco: string | null;
  numeroCuenta: string | null;
  titularCuenta: string | null;
  instrucciones: string | null;
};

export type PagoEnviado = {
  id: string;
  caseta: string;
  moneda: string;
  monto: number;
  referencia: string;
  fecha: string;
  deudas: number;
  estado: 'pendiente' | 'aprobado' | 'rechazado';
  motivoRechazo: string | null;
  numeroRecibo: number | null;
};
