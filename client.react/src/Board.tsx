import classNames from 'classnames';
import React, { useMemo } from 'react';
import './Board.css';
import Field from './Field';

export interface BoardProps {
	fields: number[][];
	highlight?: [number, number][];
	active?: boolean;
	onItemClick?: (x: number, y: number) => void;
}

const Board: React.FC<BoardProps> = ({ fields, highlight, active, onItemClick }) => {

	const renderedFields = useMemo(() => {
		function shouldHighlight(x: number, y: number) {
			return highlight != null && highlight.some(([x0, y0]) => x0 === x && y0 === y);
		}

		return fields.map((row, y) => {
			return (
				<div key={y} style={{ display: 'flex' }}>
					{row.map((item, x) => {
						return (
							<Field key={`${x},${y}`} onClick={() => onItemClick?.call(null, x, y)} highlight={shouldHighlight(x, y)} player={item} />
						);
					})}
				</div>
			);
		});
	}, [fields, highlight, onItemClick]);

	return (
		<div className={classNames({ board: true, active })}>
			{renderedFields}
		</div>
	);
};

export default Board;
