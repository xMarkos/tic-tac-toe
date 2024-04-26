import { generateId } from '@lib/idGen';
import { createContext } from 'react';
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import './App.css';

export const AppContext = createContext<{ clientId: string }>({
	clientId: (() => {
		const key = 'clientId';
		let result = sessionStorage.getItem(key);
		if (!result) {
			result = generateId(6, '0123456789abcdef');
			sessionStorage.setItem(key, result);
		}

		return result;
	})(),
});

function getBaseUrl() {
	let result = window.document.querySelector('head > base')?.getAttribute('href');
	if (!result)
		return '/';

	if (result.endsWith('/'))
		result = result.slice(0, -1);

	return result;
}

const router = createBrowserRouter([
	{
		path: '',
		async lazy() {
			return {
				Component: (await import('./Home')).default,
			};
		},
	},
	{
		path: 'lobby/:id',
		async lazy() {
			return {
				Component: (await import('./Game')).default,
			};
		},
	}
], {
	basename: getBaseUrl(),
});

export default function App() {
	console.log('rendering App');
	return (
		<>
			<h1>Tic Tac Toe (react)</h1>
			<RouterProvider router={router} />
		</>
	);
}
