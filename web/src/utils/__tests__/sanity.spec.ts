import { describe, it, expect } from 'vitest';

describe('Vitest sanity', () => {
  it('arithmetic works', () => {
    expect(1 + 1).toBe(2);
  });
  it('string equality works', () => {
    expect('hello').toBe('hello');
  });
  it('async resolves', async () => {
    await expect(Promise.resolve(42)).resolves.toBe(42);
  });
  it('arrays compare structurally', () => {
    expect([1, 2, 3]).toEqual([1, 2, 3]);
  });
  it('toThrow works', () => {
    expect(() => {
      throw new Error('boom');
    }).toThrow('boom');
  });
});
