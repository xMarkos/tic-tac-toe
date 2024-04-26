export function generateId(length: number, charPool: string): string {
	let buffer = crypto.getRandomValues(new Uint8Array(length));

	let result = '';
	for (let i = 0, l = length, m = charPool.length; i < l; i++) {
		result += charPool[buffer[i] % m];
	}

	return result;
}
