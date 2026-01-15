declare namespace API {
  interface BaseResult {
    version: string;
    code: string;
    msg: string;
    data: Record<string, unknown>;
  }
}
