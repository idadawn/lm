import { NextRequest } from "next/server";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

const AGENT_API_BASE_URL =
  process.env.NEXT_PUBLIC_AGENT_API_URL ||
  "http://127.0.0.1:18100";

function buildUpstreamUrl(path: string[], search: string): string {
  const normalizedPath = path.join("/");
  return `${AGENT_API_BASE_URL}/api/v1/kg/${normalizedPath}${search}`;
}

async function proxyRequest(
  req: NextRequest,
  context: { params: Promise<{ path: string[] }> },
): Promise<Response> {
  const { path } = await context.params;
  const upstreamUrl = buildUpstreamUrl(path, req.nextUrl.search);

  const upstream = await fetch(upstreamUrl, {
    method: req.method,
    headers: {
      "Content-Type": "application/json",
    },
    body:
      req.method === "GET" || req.method === "HEAD"
        ? undefined
        : await req.text(),
  });

  return new Response(upstream.body, {
    status: upstream.status,
    headers: {
      "Content-Type":
        upstream.headers.get("Content-Type") || "application/json",
    },
  });
}

export async function GET(
  req: NextRequest,
  context: { params: Promise<{ path: string[] }> },
): Promise<Response> {
  return proxyRequest(req, context);
}

export async function POST(
  req: NextRequest,
  context: { params: Promise<{ path: string[] }> },
): Promise<Response> {
  return proxyRequest(req, context);
}
