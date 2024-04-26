import classNames from 'classnames';
import React, { MouseEventHandler, useCallback } from 'react';
import './Field.css';

export interface FieldProps {
	player: number;
	highlight?: boolean;
	onClick?: () => void;
}

const Field: React.FC<FieldProps> = ({ player, highlight, onClick }) => {

	const clickHandler: MouseEventHandler<HTMLDivElement> = useCallback((ev) => {
		ev.stopPropagation();
		onClick && onClick();
	}, [onClick]);

	return (
		<div className={classNames({
			['player-' + player]: true,
			highlight: highlight,
			field: true,
		})} onClick={clickHandler}></div>
	);
};

export default Field;
